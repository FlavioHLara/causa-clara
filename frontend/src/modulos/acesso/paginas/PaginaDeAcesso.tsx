import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, Navigate, useLocation } from 'react-router-dom'
import { useSessao } from '../../../app/sessao'
import { mensagemDoErro } from '../../../servicos/cliente'
import { Botao } from '../../../ui/Botao'
import { Campo } from '../../../ui/Campo'
import { Faixa } from '../../../ui/Faixa'
import { esquemaDeAcesso } from '../esquemas'
import type { DadosDeAcesso } from '../esquemas'

/**
 * T01 · Acesso (RF01, RF05, RF09).
 *
 * Formulário de autenticação, caminho para o cadastro e acesso ao painel
 * público sem autenticar.
 */
export function PaginaDeAcesso() {
  const { perfil, entrar } = useSessao()
  const local = useLocation()
  const [erroDoServidor, setErroDoServidor] = useState<string | null>(null)

  // Quem acabou de se cadastrar ou de redefinir a senha chega aqui; sem o
  // aviso, a tela pareceria ter simplesmente voltado ao começo.
  const estado = local.state as { contaCriada?: boolean; senhaRedefinida?: boolean } | null
  const contaCriada = estado?.contaCriada ?? false
  const senhaRedefinida = estado?.senhaRedefinida ?? false

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<DadosDeAcesso>({ resolver: zodResolver(esquemaDeAcesso) })

  if (perfil) return <Navigate to="/painel" replace />

  async function aoEnviar(dados: DadosDeAcesso) {
    setErroDoServidor(null)
    try {
      await entrar(dados.email, dados.senha)
    } catch (erro) {
      // A API devolve "E-mail ou senha incorretos." sem dizer qual dos dois
      // falhou — a interface repassa a mensagem como veio.
      setErroDoServidor(mensagemDoErro(erro))
    }
  }

  return (
    <main className="pagina-centrada">
      <header className="marca">
        <h1>Causa Clara</h1>
        <p>Gestão e transparência para a sociedade civil</p>
      </header>

      <form className="cartao pilha" onSubmit={handleSubmit(aoEnviar)} noValidate>
        <h2>Entrar</h2>

        {contaCriada && !erroDoServidor && (
          <Faixa tipo="informacao">Conta criada. Entre com o e-mail e a senha que você cadastrou.</Faixa>
        )}

        {senhaRedefinida && !erroDoServidor && (
          <Faixa tipo="informacao">Senha redefinida. Entre com a nova senha.</Faixa>
        )}

        {erroDoServidor && <Faixa tipo="critico">{erroDoServidor}</Faixa>}

        <Campo
          rotulo="E-mail"
          type="email"
          autoComplete="email"
          placeholder="voce@exemplo.com"
          erro={errors.email?.message}
          {...register('email')}
        />

        <Campo
          rotulo="Senha"
          type="password"
          autoComplete="current-password"
          erro={errors.senha?.message}
          {...register('senha')}
        />

        <Botao type="submit" larguraCheia carregando={isSubmitting}>
          Entrar
        </Botao>

        <p className="centralizado">
          <Link to="/recuperar-senha">Esqueci minha senha</Link>
        </p>

        <hr className="divisor" />

        <p className="centralizado texto-apoio">Ainda não tem conta?</p>

        <Link to="/criar-conta" className="botao botao--contorno botao--largura-cheia">
          Criar conta
        </Link>
      </form>

      <p className="centralizado">
        <Link to="/transparencia">Consultar transparência sem entrar</Link>
      </p>

      <p className="centralizado texto-apoio">
        Os dados financeiros das instituições são públicos (RF63).
      </p>
    </main>
  )
}
