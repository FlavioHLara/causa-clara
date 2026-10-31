import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { mensagemDoErro } from '../../../servicos/cliente'
import { Botao } from '../../../ui/Botao'
import { Campo } from '../../../ui/Campo'
import { Faixa } from '../../../ui/Faixa'
import { pedirRedefinicaoDeSenha } from '../api'
import { esquemaDeRecuperacao } from '../esquemas'
import type { DadosDeRecuperacao } from '../esquemas'

/**
 * Recuperar senha — primeiro passo do RF09.
 *
 * **Esta tela não está no protótipo.** T01 prevê o link "Esqueci minha senha",
 * mas o inventário de telas não tem a tela que ele abre. Construída a partir do
 * requisito e dos tokens do design, e registrada como lacuna do protótipo.
 *
 * A confirmação é sempre a mesma, exista ou não a conta: dizer "e-mail não
 * encontrado" transformaria esta tela, que é aberta, num verificador de quem
 * tem conta cadastrada. É a mesma política do servidor, que responde 202 nos
 * dois casos.
 */
export function PaginaDeRecuperarSenha() {
  const [enviado, setEnviado] = useState(false)
  const [erroDoServidor, setErroDoServidor] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    getValues,
    formState: { errors, isSubmitting },
  } = useForm<DadosDeRecuperacao>({ resolver: zodResolver(esquemaDeRecuperacao) })

  async function aoEnviar(dados: DadosDeRecuperacao) {
    setErroDoServidor(null)
    try {
      await pedirRedefinicaoDeSenha(dados.email)
      setEnviado(true)
    } catch (erro) {
      // Só chega aqui se a requisição não completou — rede fora, servidor
      // inacessível. Falha de entrega do e-mail não vira erro: o servidor
      // responde 202 e registra no log, de propósito.
      setErroDoServidor(mensagemDoErro(erro))
    }
  }

  if (enviado) {
    return (
      <main className="pagina-centrada">
        <header className="marca">
          <h1>Verifique seu e-mail</h1>
        </header>

        <section className="cartao pilha">
          <Faixa tipo="informacao">
            Se houver uma conta com <strong>{getValues('email')}</strong>, enviamos o link de
            redefinição. O link vale por 30 minutos.
          </Faixa>

          <p className="texto-apoio">
            Não chegou? Confira a caixa de mensagens indesejadas e o endereço digitado antes de
            pedir outro link.
          </p>

          <Link to="/" className="botao botao--contorno botao--largura-cheia">
            Voltar para o acesso
          </Link>
        </section>
      </main>
    )
  }

  return (
    <main className="pagina-centrada">
      <header className="marca">
        <h1>Recuperar senha</h1>
        <p>Enviaremos um link para você escolher uma nova senha.</p>
      </header>

      <form className="cartao pilha" onSubmit={handleSubmit(aoEnviar)} noValidate>
        {erroDoServidor && <Faixa tipo="critico">{erroDoServidor}</Faixa>}

        <Campo
          rotulo="E-mail da conta"
          type="email"
          autoComplete="email"
          placeholder="voce@exemplo.com"
          erro={errors.email?.message}
          {...register('email')}
        />

        <Botao type="submit" larguraCheia carregando={isSubmitting}>
          Enviar link
        </Botao>

        <Link to="/" className="botao botao--contorno botao--largura-cheia">
          Voltar para o acesso
        </Link>
      </form>
    </main>
  )
}
