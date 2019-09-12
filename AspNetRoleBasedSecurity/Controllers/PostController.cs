using AspNetRoleBasedSecurity.Models;
using AspNetRoleBasedSecurity.Repository;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;
using System.Web.UI;


namespace AspNetRoleBasedSecurity.Controllers
{
    public class PostController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        PostRepository PostRepo = new PostRepository();      

        // GET: Post
        [Authorize]
        public ActionResult AddPost()
        {
            return View();
        }
        // POST: Employee/AddEmployee
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPost(PostModel Post)
        {
            try
            {
                if (Post.Tags != null)
                {
                    if (ModelState.IsValid)
                    {
                        PostRepository PostRepo = new PostRepository();

                        if (PostRepo.AddPost(Post))
                        {

                            SmtpClient smtpClient = new SmtpClient("hcl-com.mail.protection.outlook.com", 25);

                            smtpClient.Credentials = new System.Net.NetworkCredential("Divini.f@hcl.com", "Pacha@123");
                            smtpClient.UseDefaultCredentials = true;
                            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtpClient.EnableSsl = true;
                            MailMessage mail = new MailMessage();

                            //Setting From , To and CC
                            mail.From = new MailAddress("Divini.f@hcl.com", "Divini");
                            mail.To.Add(new MailAddress("Arun.Vijayaragavan@hcl.com"));
                            mail.CC.Add(new MailAddress("Devendran.Sh@hcl.com"));

                            smtpClient.Send(mail);

                            ViewBag.Message = "Added successfully";
                           
                        }
                        else
                        {
                            ViewBag.Message = "Duplicate Entry";
                        }
                      //  ModelState.Clear();
                    }
                }
                
                return View();                
            }
            catch
            {
                
                return View();
            }

        }

        [Authorize(Roles = "Admin")]
        // GET: Employee/GetAllPostDetails    
        public ActionResult GetAllPost(string search)
        {
            var loguser = System.Web.HttpContext.Current.User.Identity.GetUserName();
            ViewBag.Loguser = loguser;
            PostRepository PostRepo = new PostRepository();
            PostRepository PostRepo2 = new PostRepository();
            PostRepository PostRepo3 = new PostRepository();
            PostRepository PostRepo4 = new PostRepository();

            ModelState.Clear();

            var post = PostRepo.GetAllPost().OrderByDescending(Post => Post.DatePosted);
            var comments = PostRepo2.Getcomments();
            var Tags = PostRepo3.Tags();
            var popular = PostRepo.GetAllPost().OrderByDescending(Post => Post.PageView).Take(6);
            var tagfilter  = PostRepo4.Users();
            var filter = PostRepo.GetAllPost();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = (PostRepo.GetAllPost().Where(User => User.Title.StartsWith(search) || search == null).ToList());
            }

            return View(new PostDetails { Post = post, Comments = comments, popular = popular, Track = Tags , tagfilter= tagfilter, Filter= filter });
        }
        // GET: Employee/GetAllUserDetails    
        public ActionResult GetAllPostUser()
        {

            PostRepository PostRepo = new PostRepository();
            PostRepository PostRepo2 = new PostRepository();
            PostRepository PostRepo3 = new PostRepository();
            PostRepository PostRepo4 = new PostRepository();
            
            ModelState.Clear();
            var post = PostRepo.GetAllPost().OrderByDescending(Post => Post.DatePosted);
            var popular = PostRepo.GetAllPost().OrderByDescending(Post => Post.PageView).Take(6);
            var comments = PostRepo2.Getcomments();
            var Tags = PostRepo3.Tags(); var tagfilter = PostRepo4.Users();
            
            return View(new PostDetails { Post = post, Comments = comments, popular = popular, Track = Tags, tagfilter = tagfilter });
        }

        // View details from comment& Infrac tab 
        [Authorize]
        public ActionResult ViewDetails(int id, PostModel obj)
        {
            
            var userss = System.Web.HttpContext.Current.User.Identity.GetUserName();
            PostRepository PostRepo = new PostRepository();
            PostRepository PostRepo1 = new PostRepository();
            PostRepository PostRepo4 = new PostRepository();
            ViewBag.ArticleId = id;
            var comments = db.ArticlesComments.Where(d => d.ArticleId.Equals(id)).ToList();
            ViewBag.Comments = comments;
            var ratings = db.ArticlesComments.Where(d => d.ArticleId.Equals(id)).ToList();
            if (ratings.Count() > 0)
            {
                var ratingSum = ratings.Sum(d => d.Rating.Value);
                ViewBag.RatingSum = ratingSum;
                var ratingCount = ratings.Count();
                ViewBag.RatingCount = ratingCount;
            }
            else
            {
                ViewBag.RatingSum = 0;
                ViewBag.RatingCount = 0;
            }
            var tagfilter = PostRepo4.UserTag().Where(User => User.userr.Equals(userss)).Select(User => User.getProcess.ToString()).FirstOrDefault();
            //var Loginusertag = Model.tagfilter.Where(Modelitems => Modelitems.user == ViewBag.loguser).Select(Modelitems => Modelitems.getProcess.ToString()).FirstOrDefault();
            ViewBag.Tags = tagfilter;
          
            PostRepo1.ViewDetails(obj);
           
            return View(PostRepo.GetAllPost().Find((Post => Post.Id == id)));

        }
   
        // Testing code    
        [HttpPost]
        public ActionResult Updatecheck(int id, bool newValue)
        {
            var users = System.Web.HttpContext.Current.User.Identity.GetUserName();
            PostRepository PostRepo = new PostRepository();
            var dbrec = db.ArticlesComments.Find(id);

            dbrec.Status_Com = newValue;
            dbrec.Marked_by = users;
            dbrec.Marked_on = DateTime.Now;
            db.SaveChanges();

            return Json(true);
        }

        // GET:get user details from tab   
        public ActionResult UserProfile()
        {
            var users = System.Web.HttpContext.Current.User.Identity.GetUserName();
            PostRepository PostRepo = new PostRepository();
            ModelState.Clear();
            return View(PostRepo.UserProfile().OrderByDescending(Post => Post.DatePosted));

        }
         // GET: Employee/GetAllUserDetails    
        public ActionResult EditProfile()
        {
            ProfileRepository PostRepo = new ProfileRepository();
            return View(PostRepo.GetMasterUser());
        }

        [HttpPost]
        public ActionResult EditProfile(Profile Post)

        {
            try
            {
                if (ModelState.IsValid)
                {
                    PostRepository PostRepo = new PostRepository();
                    if (PostRepo.EditProfile(Post))
                    {

                        ViewBag.Message = "Added successfully";
                    }
                }
                return View();
            }
            catch
            {
                return View();
            }
        }



        public ActionResult EditPostDetails(int id)
        {
            PostRepository PostRepo = new PostRepository();



            return View(PostRepo.GetAllPost().Find((Post => Post.Id == id)));

        }

        // POST: Employee/EditEmpDetails/5    
        [HttpPost]
     
        public ActionResult EditPostDetails(int id, PostModel obj)
        {
            try
            {
                PostRepository PostRepo = new PostRepository();

                PostRepo.UpdatePost(obj);

                if (User.IsInRole("Admin"))
                {

                    return RedirectToAction("GetAllPost");
                }
                else
                {
                    return RedirectToAction("GetAllPostUser");
                }

            }
            catch
            {
                return View();
            }
        }

       
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact(int id, Tags obj, PostModel objj)
        {           
            PostRepository PostRepo = new PostRepository();
            ModelState.Clear();
         // Debug.WriteLine(PostRepo.Tags().Find((Track => Track.Id == id)));
         ///   System.Diagnostics.Trace.WriteLine(PostRepo.Tags());
            return View(PostRepo.Tags().Find((Track => Track.Id == id)));
           
        }


        public ActionResult Index()
        {
            return View();
        }

        // GET: Post/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Post/Create
        public ActionResult Create()
        {
            return View();
        }
       

        public ActionResult Users(string search)
        {
            PostRepository PostRepo = new PostRepository();
            ModelState.Clear();
            var users = PostRepo.Users();
            if (!string.IsNullOrWhiteSpace(search))
            {
                users = (PostRepo.Users().Where(User => User.user.StartsWith(search) || search == null).ToList());
            }
            return View(new AllUsers { Users = users });

        }

        

        public ActionResult Tags(string search)
        {
            PostRepository PostRepo = new PostRepository();
            ModelState.Clear();
            var track = PostRepo.Tags();
            if (!string.IsNullOrWhiteSpace(search))
            {
                track = (PostRepo.Tags().Where(User => User.Track_Name.StartsWith(search) || search == null).ToList());
            }
            return View(new Tagg { Track = track });
          
        }

        // POST: Post/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("GetAllPost");
            }
            catch
            {
                return View();
            }
        }

        // GET: Post/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Post/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

     

        public ActionResult Delete(int id,int ids)
        {
           
            ArticlesComment articlesComment = db.ArticlesComments.Find(id);
            db.ArticlesComments.Remove(articlesComment);
            db.SaveChanges();
            return RedirectToAction("ViewDetails", "Post", new { id = ids });

           
        }

        // POST: ArticlesComments/Delete/5

        public ActionResult CommentsList()
        {
            return View(db.ArticlesComments.ToList());
        }



        public ActionResult EditTag(int id)
        {
            PostRepository PostRepo = new PostRepository();
            
            return View(PostRepo.Tags().Find((Post => Post.Id == id)));

        }

        // POST: Employee/EditEmpDetails/5    
        [HttpPost]
        public ActionResult EditTag(int id, Tags obj)
        {
            try
            {
                PostRepository PostRepo = new PostRepository();

                PostRepo.UpdateTags(obj);
                return RedirectToAction("Contact", new { id = id });
            }
            catch
            {
                return View();
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        public ActionResult SendEmail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendEmail(string receiver, string subject, string message)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var fromAddress = new MailAddress("from@gmail.com", "From Name");
                    var toAddress = new MailAddress("to@yahoo.com", "To Name");
                    const string fromPassword = "password";
                    const string subjectt = "test";
                    const string body = "Hey now!!";

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                        Timeout = 20000
                    };
                    using (var mess = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subjectt,
                        Body = body
                    })
                    {
                        smtp.Send(mess);
                    }
                    return View();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Some Error";
            }
            return View();
        }

       

    }
}
