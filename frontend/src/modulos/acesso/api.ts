import { cliente, sessaoArmazenada } from '../../servicos/cliente'
import type { NovaConta, PerfilDoUsuario, SessaoAberta } from './tipos'

/** RF05: autentica por e-mail e senha. */
export async function autenticar(email: string, senha: string): Promise<SessaoAberta> {
  const { data } = await cliente.post<SessaoAberta>('/sessao', { email, senha })
  return data
}

/** RF02 a RF04: cria a conta. Não abre sessão — quem acabou de se cadastrar ainda entra. */
export async function criarConta(nova: NovaConta): Promise<PerfilDoUsuario> {
  const { data } = await cliente.post<PerfilDoUsuario>('/contas', nova)
  return data
}

/** RF07: dados cadastrais de quem está autenticado. */
export async function obterPerfil(): Promise<PerfilDoUsuario> {
  const { data } = await cliente.get<PerfilDoUsuario>('/contas/eu')
  return data
}

/**
 * RF08: encerra a sessão.
 *
 * Avisa o servidor para que o token de renovação seja revogado de fato, e
 * limpa o armazenamento local aconteça o que acontecer — sair não pode falhar
 * por causa de uma chamada de rede.
 */
export async function encerrarSessao(): Promise<void> {
  const tokenDeAtualizacao = sessaoArmazenada.tokenDeAtualizacao()

  try {
    if (tokenDeAtualizacao) {
      await cliente.delete('/sessao', { data: { tokenDeAtualizacao } })
    }
  } finally {
    sessaoArmazenada.limpar()
  }
}

/** RF09: pede o link de redefinição. A resposta é a mesma exista ou não a conta. */
export async function pedirRedefinicaoDeSenha(email: string): Promise<void> {
  await cliente.post('/senha/redefinicao', { email })
}

/**
 * RF09: troca a senha usando o segredo que chegou por e-mail.
 *
 * O servidor derruba as sessões abertas do usuário. Limpar o armazenamento
 * local aqui evita o estado confuso de a interface continuar se achando
 * autenticada com tokens que o servidor já revogou.
 */
export async function redefinirSenha(
  token: string,
  senha: string,
  confirmacaoDeSenha: string,
): Promise<void> {
  await cliente.put('/senha', { token, senha, confirmacaoDeSenha })
  sessaoArmazenada.limpar()
}
