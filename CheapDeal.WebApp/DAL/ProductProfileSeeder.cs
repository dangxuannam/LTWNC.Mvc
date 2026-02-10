using CheapDeal.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace CheapDeal.WebApp.DAL
{
    public class ProductProfileSeeder
    {
        public static void Seed(ShopDbContext context)
        {
            context.ProductProfiles.AddOrUpdate(
                p => p.ProductProfileId,

                new ProductProfile
                {
                    ProductProfileId = 1,
                    VoteCount = 150,
                    TotalScore = 720.5,
                    ViewCount = 15400,
                    Sales = 85
                },

                new ProductProfile
                {
                    ProductProfileId = 2,
                    VoteCount = 80,
                    TotalScore = 360.0,
                    ViewCount = 12000,
                    Sales = 42
                },

                new ProductProfile
                {
                    ProductProfileId = 3,
                    VoteCount = 45,
                    TotalScore = 200.0,
                    ViewCount = 8500,
                    Sales = 15
                },

                new ProductProfile
                {
                    ProductProfileId = 4,
                    VoteCount = 200,
                    TotalScore = 980.0,
                    ViewCount = 25000,
                    Sales = 120
                },

                new ProductProfile
                {
                    ProductProfileId = 5,
                    VoteCount = 30,
                    TotalScore = 120.0,
                    ViewCount = 4500,
                    Sales = 8
                },

                new ProductProfile
                {
                    ProductProfileId = 6,
                    VoteCount = 95,
                    TotalScore = 427.5,
                    ViewCount = 11000,
                    Sales = 55
                },

                new ProductProfile
                {
                    ProductProfileId = 7,
                    VoteCount = 110,
                    TotalScore = 528.0,
                    ViewCount = 14200,
                    Sales = 70
                },

                new ProductProfile
                {
                    ProductProfileId = 8,
                    VoteCount = 60,
                    TotalScore = 240.0,
                    ViewCount = 9800,
                    Sales = 30
                },

                new ProductProfile
                {
                    ProductProfileId = 9,
                    VoteCount = 25,
                    TotalScore = 112.5,
                    ViewCount = 3200,
                    Sales = 5
                },

                new ProductProfile
                {
                    ProductProfileId = 10,
                    VoteCount = 130,
                    TotalScore = 611.0,
                    ViewCount = 18000,
                    Sales = 90
                }
            );

            context.SaveChanges();
        }
    }
}