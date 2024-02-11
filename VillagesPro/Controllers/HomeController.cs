using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using VillagesPro.Models;

namespace VillagesPro.Controllers
{
    public class HomeController : Controller
    {

        private VillagesEntities appDbContext = new VillagesEntities();

        public ActionResult Index(string searchQuery, string sortOrder, int page = 1)
        {
            int pageSize = 5;
            IQueryable<VillagesList> queryable = appDbContext.VillagesLists;

            // Filter villages based on the search query
            if (!string.IsNullOrEmpty(searchQuery))
            {
                queryable = queryable.Where(v => v.VillageName.Contains(searchQuery));
            }

            // Apply sorting based on the selected sort order
            switch (sortOrder)
            {
                case "asc":
                    queryable = queryable.OrderBy(v => v.VillageName);
                    break;
                case "desc":
                    queryable = queryable.OrderByDescending(v => v.VillageName);
                    break;
                default:
                    queryable = queryable.OrderBy(v => v.VillageName);
                    break;
            }

            // Skip to the next page every 5 villegase
            var villages = queryable
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)queryable.Count() / pageSize);

            // makes sure sort order work correctly when navigate between pages
            ViewBag.SortOrder = Request.QueryString["sortOrder"];

            return View(villages);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(VillagesList village)
        {
            if (village != null && !string.IsNullOrWhiteSpace(village.VillageName))
            {
                appDbContext.VillagesLists.Add(village);
                appDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Village name cannot be empty.");
            }
            return View(village);
        }

        public ActionResult Edit(int id)
        {
            var settlement = appDbContext.VillagesLists.Find(id);
            if (settlement == null)
            {
                return HttpNotFound();
            }
            return View(settlement);
        }


        [HttpPost]
        public ActionResult EditOrDeletion(int id, string VillageName, string action)
        {
            if (ModelState.IsValid)
            {
                if (action == "save")
                {
                    // Edit logic
                    var village = appDbContext.VillagesLists.Find(id);
                    if (village == null)
                    {
                        // Create new village if not found
                        village = new VillagesList { VillageName = VillageName };
                        appDbContext.VillagesLists.Add(village);
                    }
                    else
                    {
                        // Update existing village
                        village.VillageName = VillageName;
                        appDbContext.Entry(village).State = EntityState.Modified;
                    }
                    appDbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
                else if (action == "delete")
                {
                    // Delete logic
                    var village = appDbContext.VillagesLists.Find(id);
                    if (village != null)
                    {
                        appDbContext.VillagesLists.Remove(village);
                        appDbContext.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
            }
            return View(new VillagesList { VillageName = VillageName }); // Set a blank model for re-displaying the form
        }

    }
}