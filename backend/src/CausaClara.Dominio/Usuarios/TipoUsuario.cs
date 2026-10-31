namespace CausaClara.Dominio.Usuarios;

/// <summary>
/// Tipo de conta escolhido no cadastro (RF02). Imutável depois de criado (RN01).
/// </summary>
/// <remarks>
/// Persistido como texto, não como inteiro: o banco fica legível e uma restrição
/// CHECK garante a integridade independentemente da aplicação. Renomear um membro
/// deste enum é, portanto, uma mudança de esquema — exige migração.
/// </remarks>
public enum TipoUsuario
{
    /// <summary>Registra doações e consulta a transparência das instituições.</summary>
    Apoiador,

    /// <summary>Cria instituição ou solicita acesso a uma existente, e opera lançamentos.</summary>
    Gestor
}
