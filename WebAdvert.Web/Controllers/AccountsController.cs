using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models.Accounts;
using Amazon.AspNetCore.Identity.Cognito;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;
        public AccountsController(SignInManager<CognitoUser> signInManager,
                                  UserManager<CognitoUser> userMamager, 
                                  CognitoUserPool pool)
        {
            this._signInManager = signInManager;
            this._userManager = userMamager;
            this._pool = pool;
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
                    ModelState.AddModelError("UserExists", "User with this email already exists");
                    return View(model);
                }
                //Attribute you selected as required in AWS Cognito
                user.Attributes.Add(CognitoAttribute.Name.ToString(), model.Email);
               
                var createdUser=  await _userManager.CreateAsync(user, model.Password);
                if (createdUser.Succeeded)
                {
                    RedirectToAction("Confirm");
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
                    ModelState.AddModelError("LoginError", "Email and password do not match");
                }
            }
            return View(model);
        }
    }
}
