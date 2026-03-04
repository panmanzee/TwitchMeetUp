using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using testforproject.Data;
using testforproject.Models;

namespace testforproject.Services
{
    public class RecommendationScoreResult
    {
        public Event? Event { get; set; }
        public double FinalScore { get; set; }
        public double CategoryMatchScore { get; set; }
    }

    public class RecommendationService
    {
        private readonly ApplicationDbContext _db;
        private readonly VectorCalculator _vectorCalc;

        // Weights for the recommendation formula
        private const double WeightCategoryMatch = 0.50; // 60% importance
        private const double WeightHostScore = 0.30;     // 25% importance (Max 5 stars -> normalized to 1)
        private const double WeightPopularity = 0.20;    // 15% importance (0.0 to 1.0)

        // Baseline global averages for Cold Start (Imputation)
        private const double GlobalAverageHostScore = 3.0;
        private const double GlobalAveragePopularity = 0.50;

        public RecommendationService(ApplicationDbContext db, VectorCalculator vectorCalc)
        {
            _db = db;
            _vectorCalc = vectorCalc;
        }

        // Main entry point for recommendations
        public List<RecommendationScoreResult> GetRecommendationsForUser(int userId, int limit = 10)
        {
            var user = _db.Users
                .Include(u => u.PreferredCategories)
                .FirstOrDefault(u => u.Uid == userId);

            if (user == null || user.PreferredCategories == null || !user.PreferredCategories.Any())
            {
                // Fallback: If user has no preferences, just return latest events
                return GetFallbackRecommendations(limit);
            }

            // 1. Get all Active Categories from DB to form the "Universal Vector Length" (One-Hot Encoding base)
            var allCategories = _db.Categories.OrderBy(c => c.Id).ToList();
            int vectorLength = allCategories.Count;

            // 2. Construct User Vector
            double[] userVector = BuildVector(allCategories, user.PreferredCategories.Select(c => c.Id).ToList());

            // 3. Get all Active Events (Not Expired)
            var activeEvents = _db.Events
                .Include(e => e.Categories)
                .Include(e => e.Owner)
                .Where(e => e.ExpiredDate >= DateTimeOffset.UtcNow)
                .ToList();

            var results = new List<RecommendationScoreResult>();

            // 4. Calculate score for each event
            foreach (var ev in activeEvents)
            {
                // Build Event Vector
                double[] eventVector = BuildVector(allCategories, ev.Categories.Select(c => c.Id).ToList());

                // Calculate Category Match via Cosine Similarity (0.0 to 1.0)
                double categoryMatch = _vectorCalc.CalculateCosineSimilarity(userVector, eventVector);

                // Handle Missing Data for Cold Start (HostScore)
                double rawHostScore = ev.Owner.HostScore ?? GlobalAverageHostScore; // Imputation
                double normalizedHostScore = rawHostScore / 5.0; // Normalize 0-5 stars to 0.0-1.0 range

                // Handle Missing Data for Cold Start (Popularity)
                double popularity = ev.Owner.PopularityScore ?? GlobalAveragePopularity; // Imputation

                // Calculate Final Weighted Score
                double finalScore = (WeightCategoryMatch * categoryMatch) +
                                    (WeightHostScore * normalizedHostScore) +
                                    (WeightPopularity * popularity);

                results.Add(new RecommendationScoreResult
                {
                    Event = ev,
                    FinalScore = finalScore,
                    CategoryMatchScore = categoryMatch
                });
            }

            // 5. Sort by Final Score (Descending) and take top 'limit'
            return results.OrderByDescending(r => r.FinalScore).Take(limit).ToList();
        }

        // Helper: Convert selected Category IDs into a fixed-length One-Hot Encoded array
        private double[] BuildVector(List<Category> allCategories, List<int> selectedIds)
        {
            double[] vector = new double[allCategories.Count];
            for (int i = 0; i < allCategories.Count; i++)
            {
                // If the user/event has this category, mark as 1, otherwise 0
                vector[i] = selectedIds.Contains(allCategories[i].Id) ? 1.0 : 0.0;
            }
            return vector;
        }

        private List<RecommendationScoreResult> GetFallbackRecommendations(int limit)
        {
            return _db.Events
                .Include(e => e.Owner)
                .Where(e => e.ExpiredDate >= DateTimeOffset.UtcNow)
                .OrderByDescending(e => e.EventStart)
                .Take(limit)
                .Select(e => new RecommendationScoreResult { Event = e, FinalScore = 0.0, CategoryMatchScore = 0.0 })
                .ToList();
        }
    }
}
