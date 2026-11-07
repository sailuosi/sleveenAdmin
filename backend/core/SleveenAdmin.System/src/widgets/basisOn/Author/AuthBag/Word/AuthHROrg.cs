using mooSQL.auth;

namespace SleveenAdmin.System.Author.AuthBag.Word;

public sealed class AuthHROrg : Samable
{
    public string AcountOID = string.Empty;
    public string AcountNo = string.Empty;

    public bool isSame(Samable a) =>
        a is AuthHROrg other && AcountOID == other.AcountOID;
}
