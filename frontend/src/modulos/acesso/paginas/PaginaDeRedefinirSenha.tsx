import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { mensagemDoErro } from '../../../servicos/cliente'
import { Botao } from '../../../ui/Botao'
import { Campo } from '../../../ui/Campo'
import { Faixa } from '../../../ui/Faixa'
import { redefinirSenha } from '../api'
import { esquemaDeNovaSenha, TAMANHO_MINIMO_DA_SENHA } from '../esquemas'
import type { DadosDeNovaSenha } from '../esquemas'

/**
 * Redefinir senha — segundo passo do RF09.
 *
 * **Esta tela não está no protótipo**, como a de recuperar. Construída a partir
 * do requisito e dos tokens do design.
 *
 * O segredo vem da URL, não de um campo: ele chegou no link do e-mail, e pedir
 * que a pessoa o copiasse seria transformar um detalhe de implementação em
 * tarefa do usuário.
 */
export function PaginaDeRedefinirSenha() {
  const navegar = useNavigate()
  const [parametros] = useSearchParams()
  const token = parametros.get('token')
  const [erroDoServidor, setErroDoServidor] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<DadosDeNovaSenha>({ resolver: zodResolver(esquemaDeNovaSenha) })

  // Sem token não há o que fazer, e mostrar o formulário só adiaria a frustração
  // para depois de a pessoa escolher e digitar duas vezes uma senha nova.
  if (!token) {
    return (
      <main className="pagina-centrada">
        <header className="marca">
          <h1>Link inválido</h1>
        </header>

        <section className="cartao pilha">
          <Faixa tipo="critico">
            Este endereço não traz um link de redefinição válido. Peça um novo link.
          </Faixa>

          <Link to="/recuperar-senha" className="botao botao--primario botao--largura-cheia">
            Pedir novo link
          </Link>

          <Link to="/" className="botao botao--contorno botao--largura-cheia">
            Voltar para o acesso
          </Link>
        </section>
      </main>
    )
  }

  async function aoEnviar(dados: DadosDeNovaSenha) {
    setErroDoServidor(null)
    try {
      await redefinirSenha(token!, dados.senha, dados.confirmacaoDeSenha)
      navegar('/', { replace: true, state: { senhaRedefinida: true } })
    } catch (erro) {
      // Token inexistente, expirado ou já usado chegam com a mesma mensagem do
      // servidor: os três levam à mesma ação, que é pedir outro link.
      setErroDoServidor(mensagemDoErro(erro))
    }
  }

  return (
    <main className="pagina-centrada">
      <header className="marca">
        <h1>Escolher nova senha</h1>
        <p>Ao concluir, as sessões abertas nesta conta serão encerradas.</p>
      </header>

      <form className="cartao pilha" onSubmit={handleSubmit(aoEnviar)} noValidate>
        {erroDoServidor && (
          <>
            <Faixa tipo="critico">{erroDoServidor}</Faixa>
            <Link to="/recuperar-senha">Pedir um novo link</Link>
          </>
        )}

        <Campo
          rotulo="Nova senha"
          type="password"
          autoComplete="new-password"
          dica={`Pelo menos ${TAMANHO_MINIMO_DA_SENHA} caracteres.`}
          erro={errors.senha?.message}
          {...register('senha')}
        />

        <Campo
          rotulo="Confirmar nova senha"
          type="password"
          autoComplete="new-password"
          erro={errors.confirmacaoDeSenha?.message}
          {...register('confirmacaoDeSenha')}
        />

        <Botao type="submit" larguraCheia carregando={isSubmitting}>
          Redefinir senha
        </Botao>

        <Link to="/" className="botao botao--contorno botao--largura-cheia">
          Cancelar
        </Link>
      </form>
    </main>
  )
}
