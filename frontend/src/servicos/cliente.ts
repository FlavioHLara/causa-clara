import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'

/**
 * Cliente HTTP da aplicação.
 *
 * Fala com `/api` na mesma origem: em desenvolvimento o Vite encaminha para a
 * API, e em produção o nginx faz o mesmo. Mesma origem significa nenhuma
 * configuração de CORS para manter dos dois lados.
 */
export const cliente = axios.create({ baseURL: '/api' })

/**
 * Cliente sem interceptadores, só para renovar a sessão.
 *
 * Usar o cliente principal aqui criaria recursão: a renovação responderia 401,
 * que dispararia outra renovação, e assim por diante.
 */
const clienteDeRenovacao = axios.create({ baseURL: '/api' })

const CHAVE_ACESSO = 'causa-clara.token-de-acesso'
const CHAVE_ATUALIZACAO = 'causa-clara.token-de-atualizacao'

/**
 * Guarda da sessão no navegador.
 *
 * Usa `localStorage`, com a limitação conhecida de que um ataque de script
 * injetado na página alcança o que está ali. As defesas que temos são do lado
 * do servidor e já estão no lugar: o token de acesso vive uma hora, o de
 * renovação é de uso único e rotativo, e encerrar sessão o revoga de fato.
 * Trocar por cookie `HttpOnly` exigiria a API emitir e ler cookie, o que é
 * decisão de servidor — está anotado como ponto a revisitar.
 */
export const sessaoArmazenada = {
  tokenDeAcesso: () => localStorage.getItem(CHAVE_ACESSO),
  tokenDeAtualizacao: () => localStorage.getItem(CHAVE_ATUALIZACAO),

  guardar(tokenDeAcesso: string, tokenDeAtualizacao: string) {
    localStorage.setItem(CHAVE_ACESSO, tokenDeAcesso)
    localStorage.setItem(CHAVE_ATUALIZACAO, tokenDeAtualizacao)
  },

  limpar() {
    localStorage.removeItem(CHAVE_ACESSO)
    localStorage.removeItem(CHAVE_ATUALIZACAO)
  },
}

/** Chamado quando a sessão não pode mais ser renovada, para o app reagir. */
let aoPerderSessao: () => void = () => {}

export function registrarPerdaDeSessao(callback: () => void) {
  aoPerderSessao = callback
}

cliente.interceptors.request.use((requisicao) => {
  const token = sessaoArmazenada.tokenDeAcesso()
  if (token) requisicao.headers.Authorization = `Bearer ${token}`
  return requisicao
})

/**
 * Uma renovação por vez.
 *
 * Sem isto, três requisições que recebessem 401 ao mesmo tempo dispararia três
 * renovações — e como o token de renovação é de uso único, duas falhariam e
 * derrubariam a sessão de quem estava usando o sistema normalmente.
 */
let renovacaoEmCurso: Promise<string | null> | null = null

async function renovarSessao(): Promise<string | null> {
  const tokenDeAtualizacao = sessaoArmazenada.tokenDeAtualizacao()
  if (!tokenDeAtualizacao) return null

  try {
    const { data } = await clienteDeRenovacao.post('/sessao/renovacao', {
      tokenDeAtualizacao,
    })
    sessaoArmazenada.guardar(data.tokenDeAcesso, data.tokenDeAtualizacao)
    return data.tokenDeAcesso as string
  } catch {
    sessaoArmazenada.limpar()
    return null
  }
}

type RequisicaoComRepeticao = InternalAxiosRequestConfig & { jaRepetida?: boolean }

cliente.interceptors.response.use(
  (resposta) => resposta,
  async (erro: AxiosError) => {
    const requisicao = erro.config as RequisicaoComRepeticao | undefined

    const valeRenovar =
      erro.response?.status === 401 && requisicao && !requisicao.jaRepetida

    if (!valeRenovar) return Promise.reject(erro)

    requisicao.jaRepetida = true

    renovacaoEmCurso ??= renovarSessao().finally(() => {
      renovacaoEmCurso = null
    })

    const novoToken = await renovacaoEmCurso

    if (!novoToken) {
      aoPerderSessao()
      return Promise.reject(erro)
    }

    requisicao.headers.Authorization = `Bearer ${novoToken}`
    return cliente(requisicao)
  },
)

/**
 * Mensagem de erro para o usuário.
 *
 * A API devolve ProblemDetails com `detail` já escrito em português e sem
 * informação técnica (RNF05). Quando não houver resposta — rede fora, servidor
 * inacessível —, a mensagem é nossa, e também em português.
 */
export function mensagemDoErro(erro: unknown): string {
  if (axios.isAxiosError(erro)) {
    const detalhe = (erro.response?.data as { detail?: string } | undefined)?.detail
    if (detalhe) return detalhe

    if (!erro.response) {
      return 'Não foi possível falar com o servidor. Verifique sua conexão e tente novamente.'
    }
  }

  return 'Não foi possível concluir a operação. Tente novamente em instantes.'
}
