import { Navigate } from 'react-router-dom'
import { useSessao } from '../../../app/sessao'
import { Botao } from '../../../ui/Botao'
import { Faixa } from '../../../ui/Faixa'

/**
 * Destino provisório de quem acabou de entrar.
 *
 * **Não é uma tela do protótipo.** T06 (painel do apoiador) e T10 (painel
 * administrativo) nascem nas etapas 6 e 10. Esta página existe para que a
 * fatia da etapa 3 tenha onde terminar, e para demonstrar RF07 e RF08 — ela
 * mostra os dados cadastrais que o servidor devolveu e permite encerrar a
 * sessão. Some quando os painéis de verdade chegarem.
 */
export function PaginaDoPainel() {
  const { perfil, sair } = useSessao()

  if (!perfil) return <Navigate to="/" replace />

  return (
    <main className="pagina-centrada">
      <header className="marca">
        <h1>Causa Clara</h1>
        <p>Olá, {perfil.nomeCompleto.split(' ')[0]}</p>
      </header>

      <Faixa tipo="informacao">
        Tela provisória da etapa 3. O painel do seu perfil é construído mais adiante.
      </Faixa>

      <section className="cartao pilha">
        <h2>Seus dados</h2>

        <dl className="pilha-estreita">
          <div>
            <dt className="texto-apoio">Nome completo</dt>
            <dd style={{ margin: 0 }}>{perfil.nomeCompleto}</dd>
          </div>
          <div>
            <dt className="texto-apoio">E-mail</dt>
            <dd style={{ margin: 0 }}>{perfil.email}</dd>
          </div>
          <div>
            <dt className="texto-apoio">Telefone</dt>
            <dd style={{ margin: 0 }}>{perfil.telefone ?? 'Não informado'}</dd>
          </div>
          <div>
            <dt className="texto-apoio">Tipo de conta</dt>
            <dd style={{ margin: 0 }}>{perfil.tipo}</dd>
          </div>
          <div>
            <dt className="texto-apoio">Instituição</dt>
            <dd style={{ margin: 0 }}>
              {perfil.instituicaoId
                ? `#${perfil.instituicaoId}`
                : 'Sem vínculo — o vínculo institucional entra na etapa 4'}
            </dd>
          </div>
        </dl>

        <Botao aparencia="contorno" larguraCheia onClick={() => void sair()}>
          Sair
        </Botao>
      </section>
    </main>
  )
}
