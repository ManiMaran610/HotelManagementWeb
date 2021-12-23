﻿
using HotelManagementWeb.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HotelManagementWeb.Controllers
{

    [Route("{action=index}")]

    public class DashboardController : Controller
    {
        // GET: Dashboard
        private Random randomnumber = new Random();




        public ActionResult Index()
        {
            if (User.IsInRole("Admin"))
                using (var database = new HMSContext())
                {
                    List<Room> ListOfRooms = database.Rooms.ToList();
                    List<Booking> ListofBookings = database.Bookings.ToList();
                    foreach (var data in ListOfRooms)
                    {
                       //var ExistingBookings=ListofBookings.Find(item => item.AssignRoomId == data.RoomId);
                       // if (ExistingBookings !=null && ExistingBookings.BookingFrom.Date <= DateTime.Now.Date && ExistingBookings.BookingTo.Date >= DateTime.Now.Date)
                       // {
                       //     data.Type = "Reserved";
                       // }
                     
                            data.Type = database.RoomTypes.ToList().Find(option => option.RoomTypeId == data.RoomTypeId).RoomType;
                        
                        data.Status = database.BookingStatus.ToList().Find(option => option.BookingStatusId == data.BookingStatusId).Status;
                        var model = ListOfRooms.Find(item => item.RoomId == data.RoomId);
                        model = data;
                    }
                    return View(ListOfRooms);
                }
            return new HttpNotFoundResult();
        }


        public async Task<ActionResult> Bookings(int? id)
        {
            if (User.IsInRole("Admin"))
                using (var database = new HMSContext())
                {
                    var ListOfBookings = await database.Bookings.ToListAsync();
                    decimal? TotalAmountReceived = new decimal();
                    foreach (var item in database.Bookings.ToList())
                    {
                        if (id == 1 && item.BookingTo.Date < DateTime.Now.Date)
                        {
                            ListOfBookings.Remove(item);
                        }
                        if (id == 2 && item.BookingTo.Date < DateTime.Now.Date)
                        {
                            var ExpiredBookings = database.Bookings.Where(bookings => bookings.BookingId == item.BookingId).First();
                            database.Bookings.Remove(ExpiredBookings);
                            database.SaveChangesAsync();
                        }
                        item.RoomNumber = database.Rooms.ToList().Find(room => room.RoomId == item.AssignRoomId).RoomNumber;
                        TotalAmountReceived += item.TotalAmount;
                    }
                    ViewBag.TotalAmountReceived = TotalAmountReceived;
                    return View(ListOfBookings);
                }
            return new HttpNotFoundResult();
        }


        public ActionResult Users()
        {
            if (User.IsInRole("Admin"))
                using (var database = new ApplicationDbContext())
                {
                    return View(database.Users.ToList());
                }
            return new HttpNotFoundResult();
        }

        public ActionResult DeleteUser(string Id)
        {
            if (User.IsInRole("Admin"))
                if (Id == null) return new HttpNotFoundResult();
            using (var database = new ApplicationDbContext())
            {
                var User = database.Users.Where(user => user.Id == Id).First();
                database.Users.Remove(User);
                database.SaveChanges();

                return RedirectToAction("Users");
            }
            return new HttpNotFoundResult();
        }
        [HttpGet]
        public ActionResult AddRoom(Room model)
        {
            if (User.IsInRole("Admin"))
                ViewBag.Title = "Add Room";
            ViewData["PostAction"] = "AddRoomPost";
            ViewBag.required = "required";
            using (var database = new HMSContext())
            {

                model.RoomTypeList = database.RoomTypes.ToList();
                model.BookStatusList = database.BookingStatus.ToList();
                ModelState.Clear();
                if (model.ErrorMessage != null) ModelState.AddModelError("RoomNumber", model.ErrorMessage);
                return PartialView("AddOrUpdateRoom", model);
            }
            return new HttpNotFoundResult();
        }

        public ActionResult EditRoom(Room model)
        {
            if (User.IsInRole("Admin"))
                using (var database = new HMSContext())
                {
                    model.RoomTypeList = database.RoomTypes.ToList();
                    model.BookStatusList = database.BookingStatus.ToList();
                    ViewBag.Title = "Update Room";
                    ViewData["PostAction"] = "UpdateRoomPost";
                    if (model.ErrorMessage != null) ModelState.AddModelError("RoomNumber", model.ErrorMessage);
                    return PartialView("AddOrUpdateRoom", model);
                }
            return new HttpNotFoundResult();

        }





        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddRoomPost(Room model)
        {
            if (User.IsInRole("Admin"))
                using (var database = new HMSContext())
                {
                    bool IsValid = database.Rooms.ToList().Exists(item => item.RoomNumber == model.RoomNumber);
                    if (ModelState.IsValid && !IsValid)
                    {
                        string ImageName = model.RoomNumber + randomnumber.Next() + Path.GetExtension(model.UploadedImage.FileName);
                        model.UploadedImage.SaveAs(Server.MapPath("~/RoomImages/" + ImageName));
                        model.RoomImage = ImageName;
                        model.IsActive = true;
                        database.Rooms.Add(model);
                        database.SaveChanges();
                        return RedirectToAction("index");
                    }
                    else
                    {
                        model.ErrorMessage = "Room Number already exists";
                        return RedirectToAction("AddRoom", model);
                    }
                }
            return new HttpNotFoundResult();

        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult UpdateRoomPost(Room model)
        {
            if (User.IsInRole("Admin"))
                using (var database = new HMSContext())
                {
                    bool IsValid = database.Rooms.ToList().Exists(item => item.RoomNumber == model.RoomNumber && item.RoomId != model.RoomId);
                    if (ModelState.IsValid && !IsValid)
                    {
                        if (model.UploadedImage != null)
                        {
                            FileInfo file = new FileInfo(Server.MapPath("~/RoomImages/" + model.RoomImage));
                            file.Delete();
                            string ImageName = model.RoomNumber + randomnumber.Next() + Path.GetExtension(model.UploadedImage.FileName);
                            model.UploadedImage.SaveAs(Server.MapPath("~/RoomImages/" + ImageName));
                            model.RoomImage = ImageName;
                        }
                        database.Rooms.AddOrUpdate(model);
                        database.SaveChanges();
                        return RedirectToAction("index");
                    }
                    else
                    {
                        model.ErrorMessage = "Room Number already exists with different record";
                        return RedirectToAction("EditRoom", model);
                    }
                }
            return new HttpNotFoundResult();

        }


        public ActionResult DeleteRoom(int Id)
        {
            if (User.IsInRole("Admin"))
                using (var database = new HMSContext())
                {
                    var room = database.Rooms.Where(item => item.RoomId == Id).First();
                    database.Rooms.Remove(room);
                    database.SaveChanges();
                    FileInfo file = new FileInfo(Server.MapPath("~/RoomImages/" + room.RoomImage));
                    file.Delete();
                    return RedirectToAction("Index");

                }
            return new HttpNotFoundResult();
        }
    }
}