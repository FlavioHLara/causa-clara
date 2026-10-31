import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { registrarPerdaDeSessao, sessaoArmazenada } from '../servicos/cliente'
import * as api from '../modulos/acesso/api'
import type { PerfilDoUsuario } from '../modulos/acesso/tipos'
import { ContextoDaSessao } from './sessao'
import type { Sessao } from './sessao'

/**
 * Estado da sessão para toda a aplicação.
 *
 * Ao abrir a página, se houver token guardado, o perfil é buscado no servidor
 * em vez de reconstruído do que estava no navegador: só assim a interface
 * reflete o que o servidor realmente aceita, e um token revogado cai na hora.
 */
export function SessaoProvedor({ children }: { children: ReactNode }) {
  const [perfil, setPerfil] = useState<PerfilDoUsuario | null>(null)
  const [conferindo, setConferindo] = useState(true)

  useEffect(() => {
    // O cliente HTTP avisa quando a renovação falhou e não há o que fazer.
    registrarPerdaDeSessao(() => setPerfil(null))
  }, [])

  useEffect(() => {
    let valido = true

    async function conferir() {
      if (!sessaoArmazenada.tokenDeAcesso()) {
        if (valido) setConferindo(false)
        return
      }

      try {
        const encontrado = await api.obterPerfil()
        if (valido) setPerfil(encontrado)
      } catch {
        sessaoArmazenada.limpar()
      } finally {
        if (valido) setConferindo(false)
      }
    }

    void conferir()
    return () => {
      valido = false
    }
  }, [])

  const entrar = useCallback(async (email: string, senha: string) => {
    const sessao = await api.autenticar(email, senha)
    sessaoArmazenada.guardar(sessao.tokenDeAcesso, sessao.tokenDeAtualizacao)
    setPerfil(sessao.perfil)
  }, [])

  const sair = useCallback(async () => {
    await api.encerrarSessao()
    setPerfil(null)
  }, [])

  const valor = useMemo<Sessao>(
    () => ({ perfil, conferindo, entrar, sair }),
    [perfil, conferindo, entrar, sair],
  )

  return <ContextoDaSessao.Provider value={valor}>{children}</ContextoDaSessao.Provider>
}
