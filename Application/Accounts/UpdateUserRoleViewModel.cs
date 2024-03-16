namespace Application.Accounts
{
    public class UpdateUserRoleViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public IList<string>? CurrentRoles { get; set; } = new List<string>();
    }
}
