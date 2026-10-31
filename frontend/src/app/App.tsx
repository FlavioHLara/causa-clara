import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { SessaoProvedor } from './SessaoProvedor'
import { useSessao } from './sessao'
import { PaginaEmConstrucao } from './PaginaEmConstrucao'
import { PaginaDeAcesso } from '../modulos/acesso/paginas/PaginaDeAcesso'
import { PaginaDeCriarConta } from '../modulos/acesso/paginas/PaginaDeCriarConta'
import { PaginaDeRecuperarSenha } from '../modulos/acesso/paginas/PaginaDeRecuperarSenha'
import { PaginaDeRedefinirSenha } from '../modulos/acesso/paginas/PaginaDeRedefinirSenha'
import { PaginaDoPainel } from '../modulos/acesso/paginas/PaginaDoPainel'

/**
 * Guarda de rota.
 *
 * Espelha o RNF11 para experiência de uso, e **não é a trava**: a autorização
 * de verdade é do servidor, que recusa a requisição com 401 ou 403. Aqui só se
 * evita oferecer o que será recusado.
 */
function ExigeSessao({ children }: { children: React.ReactElement }) {
  const { perfil, conferindo } = useSessao()

  // Enquanto o perfil é conferido no servidor, não dá para decidir: redirecionar
  // agora jogaria para o acesso quem tem sessão válida, a cada recarregamento.
  if (conferindo) return <p className="pagina-centrada">Carregando…</p>

  return perfil ? children : <Navigate to="/" replace />
}

export function App() {
  return (
    <BrowserRouter>
      <SessaoProvedor>
        <Routes>
          <Route path="/" element={<PaginaDeAcesso />} />
          <Route path="/criar-conta" element={<PaginaDeCriarConta />} />

          <Route
            path="/painel"
            element={
              <ExigeSessao>
                <PaginaDoPainel />
              </ExigeSessao>
            }
          />

          {/* RF09, em dois passos: pedir o link e escolher a nova senha. */}
          <Route path="/recuperar-senha" element={<PaginaDeRecuperarSenha />} />
          <Route path="/redefinir-senha" element={<PaginaDeRedefinirSenha />} />

          {/* Telas do protótipo que ainda não foram construídas. */}
          <Route
            path="/transparencia"
            element={<PaginaEmConstrucao tela="Transparência" etapa="etapa 9" />}
          />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </SessaoProvedor>
    </BrowserRouter>
  )
}
