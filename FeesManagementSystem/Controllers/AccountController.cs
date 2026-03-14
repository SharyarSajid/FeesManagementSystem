/*using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FeesManagementSystem.Models;

namespace FeesManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Signup()
        {
            ViewBag.Roles = new List<string> { "Supervisor", "Data Entry Operator" };
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Signup(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email }; // Using Email as UserName
                // You might want to store FullName in a custom User class inheriting from IdentityUser if needed, 
                // but for now we follow the standard IdentityUser.
                
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
*/


using FeesManagementSystem.Models;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FeesManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger; // Logger field add ki
        //log4net.Config.BasicConfigurator.Configure();  
        log4net.ILog log = log4net.LogManager.GetLogger(typeof(AccountController));


        private readonly Services.IEmailSender _emailSender; // Inject EmailSender

        // Constructor mein ILogger inject kiya
        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager,
                                 ILogger<AccountController> logger,
                                 Services.IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateAccount()
        {
            ViewBag.Roles = new List<string> { "Supervisor", "Data Entry Operator" };
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAccount(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                log.Info($"Attempting to create user: {model.Email}");

              

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                   log.Info($"User {model.Email} created successfully.");
                    await _userManager.AddToRoleAsync(user, model.Role);
                    // await _signInManager.SignInAsync(user, isPersistent: false); // Don't sign in immediately
                    return RedirectToAction("AccountCreated");
                }

                // Agar error aaye toh terminal mein list karein
                foreach (var error in result.Errors)
                {
                   log.Error($"CreateAccount Error for {model.Email}: {error.Code} - {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                log.Warn("CreateAccount ModelState is invalid.");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    log.Info($"Login attempt for: {model.Email}");


                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        log.Info($"User {model.Email} logged in successfully.");
                        return RedirectToAction("Index", "Home");
                    }

                    if (result.IsLockedOut)
                    {
                        log.Warn($"User {model.Email} is locked out.");
                    }
                    else
                    {
                        log.Warn($"Invalid login attempt for {model.Email}.");
                    }

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                log.Error($"Exception during login for {model.Email}: {ex.Message}");
                throw; // Rethrow the exception after logging
            }

        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            await _signInManager.SignOutAsync();
            log.Info($"User {userName} logged out.");
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                // Always return success to prevent email enumeration, unless specific requirement otherwise
                if (user == null)
                {
                    // Generic message or just redisplay for security. 
                    // For better UX during dev, we might want to show error if user not found, 
                    // but standard practice is "If email exists, OTP sent".
                    // Let's be helpful for now.
                     ModelState.AddModelError(string.Empty, "User not found.");
                     return View(model);
                }

                // Generate 4 digit OTP
                var otp = new Random().Next(1000, 9999).ToString();
                user.OtpCode = otp;
                user.OtpExpiration = DateTime.Now.AddMinutes(15); // Valid for 15 mins (Local Time)
                await _userManager.UpdateAsync(user);

                try
                {
                    await _emailSender.SendEmailAsync(model.Email, "Reset Password OTP", $"Your OTP is {otp}");
                    log.Info($"OTP generated for {model.Email}: {otp}");
                    return RedirectToAction("VerifyOtp", new { email = model.Email });
                }
                catch (Exception ex)
                {
                    log.Error($"Error sending OTP email to {model.Email}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Failed to send OTP email. Please check your internet connection and try again.");
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp(string email)
        {
            log.Info($"VerifyOtp GET called for: {email}");
            if (string.IsNullOrEmpty(email))
            {
               // Handle missing email case, maybe redirect back or show error
                ModelState.AddModelError(string.Empty, "Email is missing.");
            }
            return View(new VerifyOtpViewModel { Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.OtpCode == model.Otp && user.OtpExpiration > DateTime.Now)
                {
                     // OTP Verified
                     return RedirectToAction("ResetPassword", new { email = model.Email, token = model.Otp }); 
                     // We use OTP as token for simplicity in this custom flow, 
                     // or we can generate a secure token here if needed.
                     // The ResetPassword action will double check the OTP/Token.
                }
                ModelState.AddModelError(string.Empty, "Invalid or expired OTP.");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid password reset token.");
            }
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && user.OtpCode == model.Token && user.OtpExpiration > DateTime.Now)
                {
                    // Check if new password is same as old password
                    if (await _userManager.CheckPasswordAsync(user, model.NewPassword))
                    {
                        ModelState.AddModelError("NewPassword", "New password cannot be the same as your old password.");
                        return View(model);
                    }

                    // Reset Check Passed
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (removePasswordResult.Succeeded)
                    {
                        var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                        if (addPasswordResult.Succeeded)
                        {
                            // Clear OTP
                            user.OtpExpiration = null;
                            await _userManager.UpdateAsync(user);
                            
                            return RedirectToAction("PasswordChange"); // Or a success page
                        }
                        foreach (var error in addPasswordResult.Errors)
                        {
                             ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                     foreach (var error in removePasswordResult.Errors)
                    {
                         ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                     ModelState.AddModelError(string.Empty, "Invalid request or expired session.");
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccountCreated()
        {
            return View();
        }
    }
}