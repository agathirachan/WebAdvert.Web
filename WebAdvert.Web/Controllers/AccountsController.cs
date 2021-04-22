using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Net;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly AmazonCognitoIdentityProviderClient _cognitoIdnetityProvider;
        private readonly IConfiguration _configuration;
        private readonly CognitoUserPool _pool;
        public AccountsController(SignInManager<CognitoUser> signInManager,
                                  UserManager<CognitoUser> userMamager, 
                                  CognitoUserPool pool,
                                  AmazonCognitoIdentityProviderClient cognitoIdnetityProvider,
                                  IConfiguration configuration)
        {
            this._signInManager = signInManager;
            this._userManager = userMamager;
            this._pool = pool;
            this._cognitoIdnetityProvider = cognitoIdnetityProvider;
            this._configuration = configuration;
        }
        
        [HttpGet]
        public async Task<IActionResult> Signup()
        {
            var model = new SignupModel();
            return  View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);
                if(user.Status != null)
                {
                    ModelState.AddModelError("", "User with this email already exists");
                    return View(model);
                }
                //Attribute you selected as required in AWS Cognito
                user.Attributes.Add(CognitoAttribute.Name.ToString(), model.Email);
               
                var createdUser=  await _userManager.CreateAsync(user, model.Password);
                if (createdUser.Succeeded)
                {
                    RedirectToAction("Confirm");
                }
                if (createdUser.Errors != null && createdUser.Errors.Count() > 0)
                {
                    foreach(var error in createdUser.Errors)
                    {
                        ModelState.AddModelError(error.Code,error.Description);
                    }
                    
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Confirm()
        {
            var model = new ConfirmModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user == null)
                {
                    ModelState.AddModelError("NotFound", "A user with the given email address not found");
                    return View(model);
                }
                //var result = await _userManager.ConfirmEmailAsync(user, model.Code); //Did not work for me so used the method below
                var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code,true).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach(var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var model = new LoginModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Privacy", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email and password do not match");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPassword password)
        {
            var user = await _userManager.FindByEmailAsync(password.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                ModelState.AddModelError("", "Confirmation of password is required.");
                return View();
            }
            var clientId = _configuration.GetValue<string>("aws:UserPoolClientId");
            var userPoolClientSecret = _configuration.GetValue<string>("aws:UserPoolClientSecret");
            ForgotPasswordRequest forgotPasswordRequest = new ForgotPasswordRequest();
            forgotPasswordRequest.ClientId = clientId;
            forgotPasswordRequest.Username = user.Username;
            forgotPasswordRequest.SecretHash = CreateToken($"{user.Username}{forgotPasswordRequest.ClientId}", userPoolClientSecret);
            ForgotPasswordResponse forgotPasswordResponse = await _cognitoIdnetityProvider.ForgotPasswordAsync(forgotPasswordRequest).ConfigureAwait(false);
            if (forgotPasswordResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return RedirectToAction("ForgotPasswordConfirmation", new { email = password.Email });
            }
            else
            {
                ModelState.AddModelError("", "Failed to reset the password.");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPasswordConfirmation(string email = null)
        {
            ForgetPasswordConfirmation forgetPasswordConfirmation = new ForgetPasswordConfirmation();
            if (email != null) 
            {
                forgetPasswordConfirmation.Email = email;
            }

            return View(forgetPasswordConfirmation);
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordConfirmation(ForgetPasswordConfirmation forgetPasswordConfirmation)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = await _userManager.FindByEmailAsync(forgetPasswordConfirmation.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                ModelState.AddModelError("Email", "Problem occured with the email used");
                return View();
            }
            var clientId = _configuration.GetValue<string>("aws:UserPoolClientId");
            var userPoolClientSecret = _configuration.GetValue<string>("aws:UserPoolClientSecret");
            ConfirmForgotPasswordRequest confirmForgotPasswordRequest = new ConfirmForgotPasswordRequest();
            confirmForgotPasswordRequest.Username = user.Username;
            confirmForgotPasswordRequest.ClientId = clientId;
            confirmForgotPasswordRequest.Password = forgetPasswordConfirmation.Password;
            confirmForgotPasswordRequest.ConfirmationCode = forgetPasswordConfirmation.Code;
            confirmForgotPasswordRequest.SecretHash = CreateToken($"{user.Username}{confirmForgotPasswordRequest.ClientId}",
                                                                     userPoolClientSecret);
            ConfirmForgotPasswordResponse confirmForgotPasswordResponse = new ConfirmForgotPasswordResponse();
            string message = string.Empty;
            try
            {
                confirmForgotPasswordResponse = await _cognitoIdnetityProvider.ConfirmForgotPasswordAsync(confirmForgotPasswordRequest).ConfigureAwait(false);
            }
            catch (ExpiredCodeException ex)
            {
                message = ex.Message;
            }
            catch (InvalidPasswordException ex)
            {
                message = ex.Message;
            }
            catch (Amazon.CognitoIdentityProvider.Model.LimitExceededException ex)
            {
                message = ex.Message;
            }
            catch (UserNotFoundException ex)
            {
                message = ex.Message;
            }
            catch (UserNotConfirmedException ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError("", message);
            }
            if (confirmForgotPasswordResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                ViewData["ResetPasswordSuccess"] = "Success in resetting user password";
            }
            return View();
        }

        private string CreateToken(string message, string secret)
        {
            secret = secret ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
    }
}
