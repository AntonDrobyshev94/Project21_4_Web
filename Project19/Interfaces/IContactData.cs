using Project19.AuthContactApp;
using Project19.Entitys;
using System.Security.Claims;

namespace Project19.Interfaces
{
    public interface IContactData
    {
        IEnumerable<Contact> GetContacts();
        void AddContacts(Contact contact);
        void DeleteContact(int id);
        Task<Contact> FindContactById(int id);
        void ChangeContact(string name, string surname,
            string fatherName, string telephoneNumber, string residenceAdress, string description, int id);
        bool IsLogin(UserLoginProp model);
        bool IsRegister(UserRegistration model);
        bool AdministationRegister (UserRegistration model);
        string RoleCreate(RoleModel model);
        string AddRoleToUser(RoleModel model);
        string RemoveRoleUser(RoleModel model);
        string UserRemove(RoleModel model);

        IList<string> GetCurrentRoles();
        IList<string> GetAllUsers();
        IList<string> GetAllAdmins();
    }

}
