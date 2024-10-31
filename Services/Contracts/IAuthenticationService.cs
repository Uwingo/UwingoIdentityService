using Entity.Models;
using Entity.ModelsDto;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Services.Contracts
{
    public interface IAuthenticationService
    {
        Task<IdentityResult> RegisterUser(UserRegistrationDto userRegistrationDto);
        Task<bool> ValidateUser(UserLoginDto userLoginDto);
        Task<TokenDto> CreateToken(bool populateExp);
        Task<TokenDto> RefreshToken(TokenDto tokenDto);
        Task<User> GetUserByUserName(string userName);
        Task<List<Claim>> GetClaims(User user);
        Task<IEnumerable<User>> GetAllUsersByApplicationId(Guid applicationId);
        Task<UserDto> GetUserByIdAsync(string id);
        Task<IdentityResult> UpdateUser(UserDto user);
        Task<bool> DeleteUser(string userId);
        Task<IdentityResult> AddRoleClaimAsync(string roleId, string claimType, string claimValue);
        Task<IdentityResult> AddUserClaimAsync(string userId, string claimType, string claimValue);
        Task<IEnumerable<Claim>> GetUserClaimsAsync(string userId);
        Task<IEnumerable<Claim>> GetRoleClaimsAsync(string roleId);
        Task<IdentityResult> RemoveUserClaimAsync(string userId, string claimType, string claimValue);
        Task<IdentityResult> RemoveRoleClaimAsync(string roleId, string claimType, string claimValue);
        int GetUserCount();
        List<User> GetAllUsers();
        Task<List<UserDto>> GetPaginatedUsers(RequestParameters parameters, bool trackChanges);
        List<UserDto> GetPaginatedApplicationUsers(RequestParameters parameters, bool trackChanges, Guid applicationId);        Task<List<Claim>> GetAllClaims();
        Task<IdentityResult> UpdateUserClaimsAsync(string userId, List<ClaimDto> newClaims);
        Task<IdentityResult> UpdateRoleClaimsAsync(string roleId, List<ClaimDto> newClaims);
        Task<bool> ForgotPasswordAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<IdentityResult> ChangePassword(User user, string currentPassword, string newPassword);
        Task<List<UwingoUserDto>> GetUsersByCompanyApplication(Guid companyId, Guid applicationId);
        Task<List<RoleDto>> GetRolesByCompanyApplication(Guid companyId, Guid applicationId);
        Task<List<UwingoUser>> GetAllUsersByCompanyApplicationId(Guid companyId, Guid applicationId);
        Task<UwingoUser> GetAdminId(Guid companyId, Guid applicationId);
        Task<Role> GetAdminRoleId(Guid companyId, Guid applicationId);
        Task<int> GetAllUserCountByCompanyApplicationId(Guid companyId, Guid applicationId);
    }
}
