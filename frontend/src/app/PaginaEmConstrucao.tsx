import { Link } from 'react-router-dom'
import { Faixa } from '../ui/Faixa'

/**
 * Marca uma tela do protótipo que ainda não foi construída.
 *
 * Existe para que nenhum link da interface leve a lugar nenhum. Link quebrado
 * é pior do que link honesto: quem navega não sabe se errou o caminho ou se o
 * sistema falhou.
 */
export function PaginaEmConstrucao({ tela, etapa }: { tela: string; etapa: string }) {
  return (
    <main className="pagina-centrada">
      <header className="marca">
        <h1>{tela}</h1>
      </header>

      <Faixa tipo="atencao">
        Esta tela existe no protótipo e ainda não foi construída. Ela entra na {etapa}.
      </Faixa>

      <p className="centralizado">
        <Link to="/">Voltar para o acesso</Link>
      </p>
    </main>
  )
}
