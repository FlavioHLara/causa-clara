import { useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router-dom'
import { mensagemDoErro } from '../../../servicos/cliente'
import { Botao } from '../../../ui/Botao'
import { Campo } from '../../../ui/Campo'
import { Faixa } from '../../../ui/Faixa'
import { criarConta } from '../api'
import { esquemaDeNovaConta, TAMANHO_MINIMO_DA_SENHA } from '../esquemas'
import type { DadosDeNovaConta } from '../esquemas'
import type { TipoDeConta } from '../tipos'

/** As permissões de cada tipo, exibidas antes da confirmação (RF02). */
const TIPOS: { valor: TipoDeConta; titulo: string; descricao: string }[] = [
  {
    valor: 'Apoiador',
    titulo: 'Apoiador',
    descricao:
      'Registrar doações financeiras e materiais para instituições cadastradas e acompanhar o próprio histórico de contribuições.',
  },
  {
    valor: 'Gestor',
    titulo: 'Gestor',
    descricao:
      'Criar uma instituição ou solicitar acesso a uma existente, e administrar lançamentos financeiros e patrimoniais.',
  },
]

/**
 * T02 · Criar conta (RF02, RF03, RF04, RNF22).
 *
 * A escolha do tipo vem antes do formulário, com a descrição das permissões,
 * porque ela não pode ser alterada depois (RN01) — e o aviso de privacidade
 * fica ao pé, antes do botão que confirma.
 */
export function PaginaDeCriarConta() {
  const navegar = useNavigate()
  const [erroDoServidor, setErroDoServidor] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    control,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<DadosDeNovaConta>({
    resolver: zodResolver(esquemaDeNovaConta),
    defaultValues: { tipo: 'Apoiador' },
  })

  // useWatch em vez de watch: o segundo devolve uma função nova a cada render,
  // o que impede a memoização do componente e faz o compilador do React
  // desistir de otimizá-lo.
  const tipoEscolhido = useWatch({ control, name: 'tipo' })

  async function aoEnviar(dados: DadosDeNovaConta) {
    setErroDoServidor(null)
    try {
      await criarConta({
        nomeCompleto: dados.nomeCompleto,
        email: dados.email,
        telefone: dados.telefone?.trim() ? dados.telefone : null,
        senha: dados.senha,
        confirmacaoDeSenha: dados.confirmacaoDeSenha,
        tipo: dados.tipo,
      })

      // Cadastro não abre sessão: quem acabou de criar a conta ainda entra.
      navegar('/', { replace: true, state: { contaCriada: true } })
    } catch (erro) {
      setErroDoServidor(mensagemDoErro(erro))
    }
  }

  return (
    <main className="pagina-centrada">
      <header className="marca">
        <h1>Criar conta</h1>
        <p>Escolha o tipo de conta. Essa definição não pode ser alterada depois.</p>
      </header>

      <form className="pilha" onSubmit={handleSubmit(aoEnviar)} noValidate>
        <fieldset
          className="pilha-estreita"
          style={{ border: 0, padding: 0, margin: 0 }}
        >
          <legend className="apenas-leitor-de-tela">Tipo de conta</legend>

          {TIPOS.map((tipo) => (
            <label
              key={tipo.valor}
              className={`escolha${tipoEscolhido === tipo.valor ? ' escolha--selecionada' : ''}`}
            >
              {/* O rádio real fica escondido, mas é ele que recebe o foco e
                  responde ao teclado — o cartão é só a aparência. */}
              <input
                className="apenas-leitor-de-tela"
                type="radio"
                value={tipo.valor}
                checked={tipoEscolhido === tipo.valor}
                onChange={() => setValue('tipo', tipo.valor, { shouldValidate: true })}
                name="tipo"
              />
              <span className="escolha__marca" aria-hidden="true" />
              <span>
                <span className="escolha__titulo">{tipo.titulo}</span>
                <span className="escolha__descricao">{tipo.descricao}</span>
              </span>
            </label>
          ))}
        </fieldset>

        <div className="cartao pilha">
          {erroDoServidor && <Faixa tipo="critico">{erroDoServidor}</Faixa>}

          <Campo
            rotulo="Nome completo"
            autoComplete="name"
            erro={errors.nomeCompleto?.message}
            {...register('nomeCompleto')}
          />

          <Campo
            rotulo="Telefone (opcional)"
            type="tel"
            autoComplete="tel"
            placeholder="(31) 99999-0000"
            erro={errors.telefone?.message}
            {...register('telefone')}
          />

          <Campo
            rotulo="E-mail"
            type="email"
            autoComplete="email"
            erro={errors.email?.message}
            {...register('email')}
          />

          <Campo
            rotulo="Confirmar e-mail"
            type="email"
            autoComplete="email"
            erro={errors.confirmacaoDeEmail?.message}
            {...register('confirmacaoDeEmail')}
          />

          <Campo
            rotulo="Senha"
            type="password"
            autoComplete="new-password"
            dica={`Pelo menos ${TAMANHO_MINIMO_DA_SENHA} caracteres.`}
            erro={errors.senha?.message}
            {...register('senha')}
          />

          <Campo
            rotulo="Confirmar senha"
            type="password"
            autoComplete="new-password"
            erro={errors.confirmacaoDeSenha?.message}
            {...register('confirmacaoDeSenha')}
          />

          {/* RNF22: o aviso de privacidade aparece no cadastro, antes de confirmar. */}
          <Faixa tipo="informacao">
            Aviso de privacidade: seu nome aparecerá publicamente apenas pelo primeiro nome, ou
            como Anônimo se você optar. E-mail e telefone nunca são divulgados.
          </Faixa>

          <Botao type="submit" larguraCheia carregando={isSubmitting}>
            Criar conta
          </Botao>

          <Link to="/" className="botao botao--contorno botao--largura-cheia">
            Voltar
          </Link>
        </div>
      </form>
    </main>
  )
}
