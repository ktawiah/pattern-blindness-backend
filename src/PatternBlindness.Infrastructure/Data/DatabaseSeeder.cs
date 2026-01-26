using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Infrastructure.Data;

public static class DatabaseSeeder
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public static async Task SeedAsync(ApplicationDbContext context)
  {
    if (await context.Patterns.AnyAsync())
    {
      return; // Already seeded
    }

    // Create patterns first
    var twoPointers = CreatePattern("Two Pointers", PatternCategory.TwoPointers,
        "Use two pointers to traverse an array/string from different positions. Reduces O(n²) to O(n).",
        ["Sorted array with pair/triplet search", "In-place array modification", "Palindrome checking", "Container problems"]);

    var slidingWindow = CreatePattern("Sliding Window", PatternCategory.SlidingWindow,
        "Maintain a window that slides through data to track a subset of elements. Update incrementally as window slides.",
        ["Contiguous subarray/substring required", "Longest/shortest with constraint", "Fixed or variable window size"]);

    var binarySearch = CreatePattern("Binary Search", PatternCategory.BinarySearch,
        "Divide search space in half each iteration. Works for any problem where you can eliminate half per step.",
        ["Sorted or monotonic data", "Find exact match or boundary", "Minimize/maximize with constraint"]);

    var dfs = CreatePattern("Depth-First Search (DFS)", PatternCategory.Graph,
        "Explore as far as possible along each branch before backtracking. Uses stack (explicit or recursive).",
        ["Explore all possibilities/paths", "Tree/graph traversal", "Connected components", "Backtracking"]);

    var bfs = CreatePattern("Breadth-First Search (BFS)", PatternCategory.Graph,
        "Explore all neighbors at current depth before moving to next level. Guarantees shortest path in unweighted graphs.",
        ["Shortest path/minimum steps", "Level-by-level processing", "Nearest neighbor queries"]);

    var dp = CreatePattern("Dynamic Programming", PatternCategory.DynamicProgramming,
        "Break problem into overlapping subproblems and store solutions. Apply when you can define solution in terms of smaller subproblems.",
        ["Overlapping subproblems", "Optimal substructure", "Counting ways/paths", "Make sequence of decisions"]);

    // Add patterns to context
    await context.Patterns.AddRangeAsync(twoPointers, slidingWindow, binarySearch, dfs, bfs, dp);
    await context.SaveChangesAsync();

    // Create problems for Two Pointers
    var twoSumProblem = CreateProblem(
        "Two Sum II - Input Array Is Sorted",
        """
            Given a 1-indexed array of integers `numbers` that is already sorted in non-decreasing order, 
            find two numbers such that they add up to a specific target number.
            Return the indices of the two numbers (1-indexed) as an integer array of length 2.
            
            **Example:**
            Input: numbers = [2,7,11,15], target = 9
            Output: [1,2]
            """,
        Difficulty.Easy,
        twoPointers.Id,
        "Sorted array allows binary decisions based on current sum",
        "Start from both ends. If sum < target, move left pointer right. If sum > target, move right pointer left.",
        ["Two Pointers", "Binary Search"]);

    var containerProblem = CreateProblem(
        "Container With Most Water",
        """
            Given n non-negative integers representing vertical lines on x-axis, find two lines 
            that together with the x-axis form a container that holds the most water.
            
            **Example:**
            Input: height = [1,8,6,2,5,4,8,3,7]
            Output: 49
            """,
        Difficulty.Medium,
        twoPointers.Id,
        "Area is bounded by the shorter line - moving shorter is the only way to potentially find larger area",
        "Start with widest container (both ends). Always move the shorter pointer inward to potentially find larger area.",
        ["Two Pointers", "Greedy"]);

    // Create problems for Sliding Window
    var longestSubstringProblem = CreateProblem(
        "Longest Substring Without Repeating Characters",
        """
            Given a string s, find the length of the longest substring without repeating characters.
            
            **Example:**
            Input: s = "abcabcbb"
            Output: 3 (The answer is "abc")
            """,
        Difficulty.Medium,
        slidingWindow.Id,
        "When duplicate found, shrink window from left past the previous occurrence",
        "Expand right, track char positions. On duplicate, jump left pointer past previous occurrence.",
        ["Sliding Window", "Hash Table"]);

    var minWindowProblem = CreateProblem(
        "Minimum Window Substring",
        """
            Given strings s and t, return the minimum window substring of s that contains all characters of t.
            
            **Example:**
            Input: s = "ADOBECODEBANC", t = "ABC"
            Output: "BANC"
            """,
        Difficulty.Hard,
        slidingWindow.Id,
        "Use frequency map to track required vs current counts, shrink after finding valid",
        "Expand to find valid window, then shrink from left to minimize while maintaining validity.",
        ["Sliding Window", "Hash Table"]);

    // Create problems for Binary Search
    var rotatedArrayProblem = CreateProblem(
        "Search in Rotated Sorted Array",
        """
            Given a rotated sorted array and a target, return the index of target or -1.
            
            **Example:**
            Input: nums = [4,5,6,7,0,1,2], target = 0
            Output: 4
            """,
        Difficulty.Medium,
        binarySearch.Id,
        "One half is always sorted - use that to determine search direction",
        "Compare mid with left/right to find sorted half. Check if target is in sorted half's range to decide direction.",
        ["Binary Search"]);

    var kokoBananasProblem = CreateProblem(
        "Koko Eating Bananas",
        """
            Koko can eat at most k bananas per hour. Return the minimum k to eat all bananas in h hours.
            
            **Example:**
            Input: piles = [3,6,7,11], h = 8
            Output: 4
            """,
        Difficulty.Medium,
        binarySearch.Id,
        "Binary search on the answer - relationship between speed and hours is monotonic",
        "Search space is [1, max_pile]. For each speed k, calculate total hours using ceiling division per pile.",
        ["Binary Search"]);

    // Create problems for DFS
    var islandsProblem = CreateProblem(
        "Number of Islands",
        """
            Given a 2D grid of '1's (land) and '0's (water), count the number of islands.
            
            **Example:**
            Input: grid = [["1","1","0"],["1","0","0"],["0","0","1"]]
            Output: 2
            """,
        Difficulty.Medium,
        dfs.Id,
        "Each DFS explores one complete island - mark visited to avoid recounting",
        "Iterate grid, start DFS on unvisited land. DFS marks all connected land as visited. Count DFS starts.",
        ["DFS", "BFS", "Union Find"]);

    var pathSumProblem = CreateProblem(
        "Path Sum II",
        """
            Find all root-to-leaf paths where each path's sum equals targetSum.
            
            **Example:**
            Input: root = [5,4,8,11,null,13,4,7,2,null,null,5,1], targetSum = 22
            Output: [[5,4,11,2],[5,8,4,5]]
            """,
        Difficulty.Medium,
        dfs.Id,
        "Path must end at leaf (both children null) - use backtracking to build paths",
        "DFS with path tracking. Add node to path, recurse on children. At leaf, check sum. Backtrack by removing node.",
        ["DFS", "Backtracking", "Tree"]);

    // Create problems for BFS
    var levelOrderProblem = CreateProblem(
        "Binary Tree Level Order Traversal",
        """
            Return the level order traversal of a binary tree (left to right, level by level).
            
            **Example:**
            Input: root = [3,9,20,null,null,15,7]
            Output: [[3],[9,20],[15,7]]
            """,
        Difficulty.Medium,
        bfs.Id,
        "Capture queue size at level start to know where level ends",
        "Use queue. For each level: save queue size, process exactly that many nodes, children go to next level.",
        ["BFS", "Tree"]);

    var rottingOrangesProblem = CreateProblem(
        "Rotting Oranges",
        """
            Every minute, fresh oranges adjacent to rotten ones become rotten.
            Return minutes until no fresh oranges remain, or -1 if impossible.
            
            **Example:**
            Input: grid = [[2,1,1],[1,1,0],[0,1,1]]
            Output: 4
            """,
        Difficulty.Medium,
        bfs.Id,
        "Multi-source BFS - all rotten oranges spread simultaneously",
        "Add ALL rotten oranges to queue initially. BFS level = 1 minute. After BFS, check for remaining fresh.",
        ["BFS", "Matrix"]);

    // Create problems for DP
    var climbingStairsProblem = CreateProblem(
        "Climbing Stairs",
        """
            You can climb 1 or 2 steps at a time. How many distinct ways to climb n stairs?
            
            **Example:**
            Input: n = 3
            Output: 3 (1+1+1, 1+2, 2+1)
            """,
        Difficulty.Easy,
        dp.Id,
        "dp[n] = dp[n-1] + dp[n-2] - ways to reach step n from step n-1 or n-2",
        "Base: dp[0]=1 (one way to stay), dp[1]=1. Recurrence: dp[i] = dp[i-1] + dp[i-2]. Can optimize to O(1) space.",
        ["Dynamic Programming", "Math"]);

    var houseRobberProblem = CreateProblem(
        "House Robber",
        """
            Rob houses with max money without robbing adjacent houses.
            
            **Example:**
            Input: nums = [2,7,9,3,1]
            Output: 12 (rob houses 0, 2, 4: 2+9+1=12)
            """,
        Difficulty.Medium,
        dp.Id,
        "At each house: max of (skip it, rob it + prev non-adjacent)",
        "dp[i] = max(dp[i-1], dp[i-2] + nums[i]). Skip current = dp[i-1]. Rob current = dp[i-2] + nums[i].",
        ["Dynamic Programming"]);

    var lisProblem = CreateProblem(
        "Longest Increasing Subsequence",
        """
            Return the length of the longest strictly increasing subsequence.
            
            **Example:**
            Input: nums = [10,9,2,5,3,7,101,18]
            Output: 4 ([2,3,7,101])
            """,
        Difficulty.Medium,
        dp.Id,
        "dp[i] = LIS ending at index i. For each j < i where nums[j] < nums[i], consider dp[j] + 1",
        "dp[i] = max(dp[j] + 1) for all j < i where nums[j] < nums[i]. Initialize all dp[i] = 1. Can optimize to O(n log n) with binary search.",
        ["Dynamic Programming", "Binary Search"]);

    // Add all problems
    await context.Problems.AddRangeAsync(
        twoSumProblem, containerProblem,
        longestSubstringProblem, minWindowProblem,
        rotatedArrayProblem, kokoBananasProblem,
        islandsProblem, pathSumProblem,
        levelOrderProblem, rottingOrangesProblem,
        climbingStairsProblem, houseRobberProblem, lisProblem);
    await context.SaveChangesAsync();

    // Add wrong approaches to problems
    AddWrongApproachesToTwoPointerProblems(twoSumProblem, containerProblem, slidingWindow.Id, dp.Id);
    AddWrongApproachesToSlidingWindowProblems(longestSubstringProblem, minWindowProblem, twoPointers.Id, dp.Id);
    AddWrongApproachesToBinarySearchProblems(rotatedArrayProblem, kokoBananasProblem, twoPointers.Id, dp.Id);
    AddWrongApproachesToDfsProblems(islandsProblem, pathSumProblem, bfs.Id, dp.Id);
    AddWrongApproachesToBfsProblems(levelOrderProblem, rottingOrangesProblem, dfs.Id);
    AddWrongApproachesToDpProblems(climbingStairsProblem, houseRobberProblem, lisProblem, binarySearch.Id);

    await context.SaveChangesAsync();
  }

  private static Pattern CreatePattern(string name, PatternCategory category, string description, string[] signals)
  {
    return Pattern.Create(
        name,
        description,
        category,
        JsonSerializer.Serialize(signals, JsonOptions));
  }

  private static Problem CreateProblem(
      string title,
      string description,
      Difficulty difficulty,
      Guid correctPatternId,
      string keyInvariant,
      string solutionExplanation,
      string[] tags)
  {
    return Problem.Create(
        title,
        description,
        difficulty,
        correctPatternId,
        keyInvariant,
        solutionExplanation,
        JsonSerializer.Serialize(tags, JsonOptions));
  }

  private static void AddWrongApproachesToTwoPointerProblems(Problem twoSum, Problem container, Guid slidingWindowId, Guid dpId)
  {
    // Two Sum II wrong approaches
    twoSum.AddWrongApproach(slidingWindowId,
        "Using hash map ignores the sorted property - works but O(n) space vs O(1) with two pointers", 25);
    twoSum.AddWrongApproach(dpId,
        "Brute force nested loops - O(n²) fails to leverage sorted property for O(n) solution", 40);

    // Container wrong approaches  
    container.AddWrongApproach(dpId,
        "Checking all pairs with nested loops - O(n²) when O(n) two pointers works", 35);
    container.AddWrongApproach(slidingWindowId,
        "Moving the taller pointer inward - only moving shorter pointer can increase area", 30);
  }

  private static void AddWrongApproachesToSlidingWindowProblems(Problem longest, Problem minWindow, Guid twoPointersId, Guid dpId)
  {
    // Longest substring wrong approaches
    longest.AddWrongApproach(dpId,
        "Checking all substrings O(n²) - sliding window maintains valid state incrementally", 30);
    longest.AddWrongApproach(twoPointersId,
        "Shrinking window by 1 when duplicate found - should jump to after previous occurrence", 25);

    // Min window wrong approaches
    minWindow.AddWrongApproach(twoPointersId,
        "Using a set instead of frequency map - fails when t has duplicate characters", 35);
    minWindow.AddWrongApproach(dpId,
        "Not shrinking window after finding valid substring - misses smaller valid windows", 25);
  }

  private static void AddWrongApproachesToBinarySearchProblems(Problem rotated, Problem koko, Guid twoPointersId, Guid dpId)
  {
    // Rotated array wrong approaches
    rotated.AddWrongApproach(twoPointersId,
        "Finding rotation point first then binary search - two passes when one suffices", 20);
    rotated.AddWrongApproach(dpId,
        "Linear search fallback - binary search still works with modified conditions", 15);

    // Koko bananas wrong approaches
    koko.AddWrongApproach(twoPointersId,
        "Linear search from 1 to max - O(max_pile * n) vs O(n log max_pile)", 35);
    koko.AddWrongApproach(dpId,
        "Using average pile size - doesn't account for ceiling division per pile", 25);
  }

  private static void AddWrongApproachesToDfsProblems(Problem islands, Problem pathSum, Guid bfsId, Guid dpId)
  {
    // Islands wrong approaches
    islands.AddWrongApproach(dpId,
        "Forgetting to mark cells as visited - infinite loop or counting same island multiple times", 40);
    islands.AddWrongApproach(bfsId,
        "Both DFS and BFS work here - pick what you're most comfortable with", 10);

    // Path Sum II wrong approaches
    pathSum.AddWrongApproach(dpId,
        "Not checking if node is a leaf - counts internal nodes with correct sum as valid paths", 30);
    pathSum.AddWrongApproach(bfsId,
        "Adding path reference instead of copy to result - all results point to same mutated list", 35);
  }

  private static void AddWrongApproachesToBfsProblems(Problem levelOrder, Problem rotting, Guid dfsId)
  {
    // Level order wrong approaches
    levelOrder.AddWrongApproach(dfsId,
        "Using DFS without tracking depth - nodes end up in wrong levels or mixed together", 35);

    // Rotting oranges wrong approaches
    rotting.AddWrongApproach(dfsId,
        "Starting BFS from one rotten orange at a time - need multi-source BFS for simultaneous spread", 40);
    rotting.AddWrongApproach(dfsId,
        "Using DFS - doesn't model simultaneous spreading correctly", 25);
  }

  private static void AddWrongApproachesToDpProblems(Problem climbing, Problem robber, Problem lis, Guid binarySearchId)
  {
    // Climbing stairs wrong approaches
    climbing.AddWrongApproach(binarySearchId,
        "Pure recursion without memoization - O(2^n) exponential time", 40);

    // House robber wrong approaches
    robber.AddWrongApproach(binarySearchId,
        "Greedy: always rob every other house - skipping 2 houses might be optimal", 35);

    // LIS wrong approaches
    lis.AddWrongApproach(binarySearchId,
        "Confusing subsequence with subarray - subsequence doesn't need to be contiguous", 25);
  }
}
