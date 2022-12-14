using Front_to_back.Models;
using Front_to_back.Services.Interfaces;
using Front_to_back.ViewModels.AccountViewModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Front_to_back.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, 
            IEmailService emailService, 
            IFileService fileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _fileService = fileService;
        }

       

        [HttpGet]
        public IActionResult Qeydiyyat()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Qeydiyyat(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            AppUser user = new AppUser
            {
                FullName = registerVM.Fullname,
                UserName = registerVM.Username,
                Email = registerVM.Email,
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(registerVM);

            }

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            string link = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token },
                Request.Scheme, Request.Host.ToString());

            
            string body = string.Empty;
            string path = "wwwroot/assets/templates/verify.html";
            string subject = "Verify Email";

            body = _fileService.ReadFile(path, body);

             

            body = body .Replace("{{UserName}}", user.FullName);
            body = body .Replace("{{link}}", link);

            _emailService.Send(user.Email, subject, body);
           


            //await _signInManager.SignInAsync(user, false);

            return RedirectToAction(nameof(VerifyEmail));
        }


        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null) return BadRequest();

            AppUser user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            await _userManager.ConfirmEmailAsync(user, token);

            await _signInManager.SignInAsync(user,false);

            return RedirectToAction("Index", "Home");
        }


        public IActionResult VerifyEmail()
        {
            return View();
        }




        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);

            AppUser user = await _userManager.FindByEmailAsync(loginVM.UsernameOrEmail);

            if(user is null)
            {
                user = await _userManager.FindByNameAsync(loginVM.UsernameOrEmail);
            }

            if(user is null)
            {
                ModelState.AddModelError("", "Email or password wrong");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Email or password wrong");
                return View(loginVM);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
