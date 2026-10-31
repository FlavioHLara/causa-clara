import type { ReactNode } from 'react'
import './ui.css'

type Tipo = 'informacao' | 'atencao' | 'critico'

const SIMBOLO: Record<Tipo, string> = {
  informacao: 'ℹ',
  atencao: '⚠',
  critico: '⨯',
}

const ROTULO: Record<Tipo, string> = {
  informacao: 'Informação:',
  atencao: 'Atenção:',
  critico: 'Erro:',
}

/**
 * Faixa de aviso.
 *
 * RNF08: a situação é comunicada por cor, símbolo **e** rótulo textual. O
 * rótulo fica disponível ao leitor de tela mesmo quando não aparece, para que
 * a natureza do aviso não dependa de enxergar a cor da borda.
 */
export function Faixa({ tipo, children }: { tipo: Tipo; children: ReactNode }) {
  return (
    <div
      className={`faixa faixa--${tipo}`}
      role={tipo === 'critico' ? 'alert' : 'status'}
    >
      <span className="faixa__simbolo" aria-hidden="true">
        {SIMBOLO[tipo]}
      </span>
      <p>
        <span className="apenas-leitor-de-tela">{ROTULO[tipo]} </span>
        {children}
      </p>
    </div>
  )
}
