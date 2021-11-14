﻿using CinemaManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Models.Services
{
    public class BillService
    {
        private static BillService _ins;
        public static BillService Ins
        {
            get
            {
                if (_ins == null)
                {
                    _ins = new BillService();
                }
                return _ins;
            }
            private set => _ins = value;
        }
        private BillService()
        {
        }
        public List<BillDTO> GetBillByDate(DateTime date) {
            try
            {
                var context = DataProvider.Ins.DB;


                var billList = (from b in context.Bills
                        where DbFunctions.TruncateTime(b.CreatedAt) == date.Date
                        select new BillDTO
                        {
                            Id = b.Id,
                            StaffId = b.StaffId,
                            StaffName = b.Staff.Name,
                            TotalPrice = b.TotalPrice,
                            DiscountPrice = b.DiscountPrice,
                            CustomerId =b.CustomerId,
                            CustomerName =b.Customer.Name,
                        }).ToList();
                return billList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public BillDTO GetBillDetails(string billId)
        {
            try
            {
                var context = DataProvider.Ins.DB;
                var bill= context.Bills.Find(billId);

                BillDTO billInfo = new BillDTO
                {
                    Id = bill.Id,
                    StaffId = bill.Staff.Id,
                    StaffName = bill.Staff.Name,
                    DiscountPrice = bill.DiscountPrice,
                    TotalPrice = bill.TotalPrice,
                    ProductBillInfoes = (from pi in bill.ProductBillInfoes
                                         select new ProductBillInfoDTO
                                         {
                                             BillId = pi.BillId,
                                             ProductId = pi.ProductId,
                                             ProductName = pi.Product.DisplayName,
                                             PricePerItem = pi.PricePerItem,
                                             Quantity = pi.Quantity
                                         }).ToList(),
                };
                if (bill.CustomerId !=  null)
                {
                    billInfo.CustomerId = bill.Customer.Id;
                    billInfo.CustomerName = bill.Customer.Name;
                    billInfo.PhoneNumber = bill.Customer.PhoneNumber;
                }
                

                var tickets = bill.Tickets;
                var showtime = tickets.FirstOrDefault().Showtime;
                int roomId = 0;
                List<string> seatList = new List<string>();
                foreach (var t in tickets)
                {
                    if (roomId == 0)
                    {
                        roomId = t.Seat.RoomId;
                    }
                    seatList.Add($"{t.Seat.Row}{t.Seat.SeatNumber}");
                }

                billInfo.TicketInfo = new TicketBillInfoDTO() {
                    roomId = roomId,
                    movieName = showtime.Movie.DisplayName,
                    ShowDate = showtime.ShowtimeSetting.ShowDate,
                    StartShowTime = showtime.StartTime,
                    TotalPriceTicket = tickets.Count() * showtime.TicketPrice,
                    seats = seatList,
                };
                return billInfo;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
