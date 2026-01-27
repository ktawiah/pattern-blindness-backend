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
        // Always update patterns to ensure they have comprehensive data
        await SeedPatternsAsync(context);

        if (!await context.DataStructures.AnyAsync())
        {
            await SeedDataStructuresAsync(context);
        }
    }

    private static async Task SeedPatternsAsync(ApplicationDbContext context)
    {
        var patterns = CreatePatterns();

        foreach (var newPattern in patterns)
        {
            var existingPattern = await context.Patterns
                .FirstOrDefaultAsync(p => p.Name == newPattern.Name);

            if (existingPattern == null)
            {
                await context.Patterns.AddAsync(newPattern);
            }
            else
            {
                // Update existing pattern with new comprehensive data using Update method
                existingPattern.Update(
                    name: newPattern.Name,
                    description: newPattern.Description,
                    whatItIs: newPattern.WhatItIs,
                    whenToUse: newPattern.WhenToUse,
                    whyItWorks: newPattern.WhyItWorks,
                    commonUseCases: newPattern.CommonUseCases,
                    timeComplexity: newPattern.TimeComplexity,
                    spaceComplexity: newPattern.SpaceComplexity,
                    pseudoCode: newPattern.PseudoCode,
                    triggerSignals: newPattern.TriggerSignals,
                    commonMistakes: newPattern.CommonMistakes,
                    resources: newPattern.Resources,
                    relatedPatternIds: newPattern.RelatedPatternIds
                );
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDataStructuresAsync(ApplicationDbContext context)
    {
        var dataStructures = CreateDataStructures();
        await context.DataStructures.AddRangeAsync(dataStructures);
        await context.SaveChangesAsync();
    }

    private static string ToJson<T>(T obj) => JsonSerializer.Serialize(obj, JsonOptions);

    private static List<Pattern> CreatePatterns()
    {
        return
        [
            // Two Pointers Pattern
            Pattern.Create(
                name: "Two Pointers",
                description: "Use two pointers to traverse a data structure, typically from opposite ends or at different speeds.",
                category: PatternCategory.TwoPointers,
                whatItIs: @"Two Pointers is a technique where you use two references (pointers) to traverse a data structure. The pointers can:
- Start from opposite ends and move toward each other
- Move at different speeds (slow/fast pointer)
- Both start from the same end moving in the same direction

This technique reduces the need for nested loops, often improving time complexity from O(n²) to O(n).",
                whenToUse: @"Use Two Pointers when:
- The input is sorted or has some ordering property
- You need to find pairs that satisfy a condition
- You need to compare elements from different positions
- Working with palindromes or symmetric structures
- Detecting cycles in linked lists",
                whyItWorks: @"Two Pointers works because:
1. Sorted data allows making decisions based on comparisons
2. Moving pointers based on conditions eliminates unnecessary comparisons
3. The approach leverages the structure of the data to skip invalid combinations",
                commonUseCases: ToJson(new[] { "Two Sum in sorted array", "Container with most water", "Palindrome check", "Remove duplicates", "Merge sorted arrays" }),
                timeComplexity: "O(n)",
                spaceComplexity: "O(1)",
                pseudoCode: @"left = 0
right = len(arr) - 1

while left < right:
    if condition_met(arr[left], arr[right]):
        return result
    elif need_larger_value:
        left += 1
    else:
        right -= 1",
                triggerSignals: ToJson(new[] { "Sorted array/string", "Find pair with sum/difference", "Palindrome", "In-place modification", "Comparing from both ends" }),
                commonMistakes: ToJson(new[] { "Off-by-one errors with indices", "Not handling duplicates properly", "Wrong pointer movement direction", "Missing edge cases (empty array, single element)" }),
                resources: ToJson(new object[] {
                    new { title = "Two Pointers - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/4", type = "course" },
                    new { title = "Two Pointers Pattern - LeetCode", url = "https://leetcode.com/explore/interview/card/leetcodes-interview-crash-course-data-structures-and-algorithms/703/arraystrings/4501/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Sliding Window Pattern
            Pattern.Create(
                name: "Sliding Window",
                description: "Maintain a window of elements that slides through the data structure to find optimal subarrays/substrings.",
                category: PatternCategory.SlidingWindow,
                whatItIs: @"Sliding Window maintains a contiguous subset (window) of elements and slides it through the input. Two types:
- Fixed size: Window size stays constant
- Variable size: Window expands/contracts based on conditions

The window typically tracks a running state (sum, count, set of characters) that updates incrementally.",
                whenToUse: @"Use Sliding Window when:
- Looking for subarrays/substrings satisfying a condition
- Need to find min/max length subarray with some property
- Working with contiguous sequences
- The problem mentions 'consecutive' or 'contiguous'",
                whyItWorks: @"Sliding Window works because:
1. Incrementally updating window state is O(1) vs recomputing from scratch
2. The window maintains invariants that help identify valid solutions
3. Expansion/contraction logic systematically explores all possibilities",
                commonUseCases: ToJson(new[] { "Longest substring without repeats", "Minimum window substring", "Maximum sum subarray of size k", "Find all anagrams", "Longest subarray with sum ≤ k" }),
                timeComplexity: "O(n)",
                spaceComplexity: "O(k) where k is window size or character set",
                pseudoCode: @"left = 0
window_state = initial_state

for right in range(len(arr)):
    # Expand window
    update_state(arr[right])
    
    # Contract window while invalid
    while window_invalid():
        remove_from_state(arr[left])
        left += 1
    
    # Update result
    result = max(result, right - left + 1)",
                triggerSignals: ToJson(new[] { "Contiguous subarray/substring", "Maximum/minimum length", "Fixed window size k", "Substring containing all characters", "At most k distinct" }),
                commonMistakes: ToJson(new[] { "Forgetting to shrink window when needed", "Not handling empty window case", "Off-by-one in window size calculation", "Incorrect state update when elements leave window" }),
                resources: ToJson(new object[] {
                    new { title = "Sliding Window - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/1", type = "course" },
                    new { title = "Sliding Window for Beginners", url = "https://leetcode.com/discuss/general-discussion/657507/sliding-window-for-beginners-problems-template-sample-solutions", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Binary Search Pattern
            Pattern.Create(
                name: "Binary Search",
                description: "Repeatedly divide the search space in half to find target or optimal value in logarithmic time.",
                category: PatternCategory.BinarySearch,
                whatItIs: @"Binary Search divides a sorted space in half with each comparison. Beyond finding exact values, it can:
- Find insertion position
- Search for first/last occurrence
- Search on answer (binary search the solution space)

Key insight: If the search space has a monotonic property, binary search applies.",
                whenToUse: @"Use Binary Search when:
- Input is sorted or has monotonic property
- Need to find exact value, boundary, or optimal answer
- Search space can be clearly defined with low/high bounds
- A function f(x) transitions from false to true (or vice versa)",
                whyItWorks: @"Binary Search works because:
1. Each comparison eliminates half the remaining search space
2. Monotonicity ensures the answer lies in one half after each check
3. Log₂(n) halvings are needed to reach a single element",
                commonUseCases: ToJson(new[] { "Search in sorted array", "Find first/last position", "Search in rotated array", "Koko eating bananas", "Capacity to ship packages", "Square root" }),
                timeComplexity: "O(log n)",
                spaceComplexity: "O(1) iterative, O(log n) recursive",
                pseudoCode: @"def binary_search(arr, target):
    left, right = 0, len(arr) - 1
    
    while left <= right:
        mid = left + (right - left) // 2
        
        if arr[mid] == target:
            return mid
        elif arr[mid] < target:
            left = mid + 1
        else:
            right = mid - 1
    
    return -1  # or left for insertion point",
                triggerSignals: ToJson(new[] { "Sorted array", "Find minimum/maximum that satisfies condition", "Search space with clear bounds", "Minimize maximum or maximize minimum", "O(log n) requirement" }),
                commonMistakes: ToJson(new[] { "Integer overflow in mid calculation", "Wrong boundary update (mid vs mid±1)", "Incorrect loop condition (< vs <=)", "Not handling empty array", "Off-by-one in result" }),
                resources: ToJson(new object[] {
                    new { title = "Binary Search - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/2", type = "course" },
                    new { title = "Binary Search 101", url = "https://leetcode.com/problems/binary-search/solutions/423162/Binary-Search-101/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // DFS Pattern
            Pattern.Create(
                name: "Depth-First Search (DFS)",
                description: "Explore as far as possible along each branch before backtracking, used for trees, graphs, and recursive exploration.",
                category: PatternCategory.Graph,
                whatItIs: @"DFS explores a graph or tree by going as deep as possible before backtracking. It uses:
- Recursion (implicit stack) or explicit stack
- Visited set to avoid cycles in graphs
- Can be preorder, inorder, or postorder for trees",
                whenToUse: @"Use DFS when:
- Need to explore all paths or possibilities
- Finding connected components
- Detecting cycles
- Path finding where path matters (not just reachability)
- Tree traversals
- Backtracking problems",
                whyItWorks: @"DFS works because:
1. The stack (implicit or explicit) naturally handles the backtracking
2. Systematic exploration ensures all reachable nodes are visited
3. Memory efficient for deep graphs (only stores current path)",
                commonUseCases: ToJson(new[] { "Number of islands", "Path sum in tree", "Clone graph", "Course schedule", "Word search", "Generate permutations" }),
                timeComplexity: "O(V + E) for graphs, O(n) for trees",
                spaceComplexity: "O(h) where h is height/depth",
                pseudoCode: @"def dfs(node, visited):
    if node is None or node in visited:
        return
    
    visited.add(node)
    # Process node
    
    for neighbor in node.neighbors:
        dfs(neighbor, visited)",
                triggerSignals: ToJson(new[] { "Explore all paths", "Connected components", "Tree traversal", "Detect cycle", "Path exists", "Generate combinations" }),
                commonMistakes: ToJson(new[] { "Forgetting to mark visited", "Not handling cycles in graphs", "Wrong base case", "Stack overflow on deep recursion", "Modifying collection while iterating" }),
                resources: ToJson(new object[] {
                    new { title = "Graph DFS - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/7", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // BFS Pattern
            Pattern.Create(
                name: "Breadth-First Search (BFS)",
                description: "Explore all neighbors at present depth before moving to nodes at next depth level, ideal for shortest paths.",
                category: PatternCategory.Graph,
                whatItIs: @"BFS explores a graph level by level using a queue. Key properties:
- First path found to any node is the shortest (in unweighted graphs)
- Processes all nodes at distance d before distance d+1
- Can be used for level-order traversal of trees",
                whenToUse: @"Use BFS when:
- Finding shortest path in unweighted graph
- Level-order traversal needed
- Finding minimum steps/moves
- Multi-source shortest path
- Problems involving 'nearest' or 'minimum distance'",
                whyItWorks: @"BFS works because:
1. The queue ensures FIFO processing (first discovered = first processed)
2. Level-by-level exploration guarantees shortest path in unweighted graphs
3. All nodes at distance d are processed before distance d+1",
                commonUseCases: ToJson(new[] { "Shortest path in maze", "Level order traversal", "Rotting oranges", "Word ladder", "Minimum knight moves", "Nearest exit in maze" }),
                timeComplexity: "O(V + E) for graphs, O(n) for trees",
                spaceComplexity: "O(V) for the queue",
                pseudoCode: @"from collections import deque

def bfs(start):
    queue = deque([start])
    visited = {start}
    level = 0
    
    while queue:
        for _ in range(len(queue)):  # Process current level
            node = queue.popleft()
            # Process node
            
            for neighbor in node.neighbors:
                if neighbor not in visited:
                    visited.add(neighbor)
                    queue.append(neighbor)
        level += 1",
                triggerSignals: ToJson(new[] { "Shortest path unweighted", "Minimum steps/moves", "Level-order traversal", "Nearest/closest", "Spreading/infection simulation" }),
                commonMistakes: ToJson(new[] { "Using DFS for shortest path", "Not tracking visited before enqueueing", "Processing level boundaries incorrectly", "Forgetting to handle disconnected components" }),
                resources: ToJson(new object[] {
                    new { title = "Graph BFS - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/8", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Dynamic Programming Pattern
            Pattern.Create(
                name: "Dynamic Programming",
                description: "Break down problems into overlapping subproblems and store solutions to avoid redundant computation.",
                category: PatternCategory.DynamicProgramming,
                whatItIs: @"Dynamic Programming (DP) solves problems by:
1. Breaking them into smaller overlapping subproblems
2. Solving each subproblem once and storing the result
3. Building up to the final solution

Two approaches: Top-down (memoization) and Bottom-up (tabulation).",
                whenToUse: @"Use DP when:
- Problem has optimal substructure (optimal solution contains optimal solutions to subproblems)
- Problem has overlapping subproblems (same subproblems solved multiple times)
- Need to count ways, find min/max, or make optimal decisions",
                whyItWorks: @"DP works because:
1. Storing solutions eliminates redundant computation
2. Optimal substructure means we can build optimal solution from subproblem solutions
3. Systematic approach ensures all possibilities are considered",
                commonUseCases: ToJson(new[] { "Fibonacci", "Climbing stairs", "House robber", "Longest common subsequence", "Knapsack", "Edit distance", "Coin change" }),
                timeComplexity: "Varies: O(n), O(n²), O(n×m), etc.",
                spaceComplexity: "O(n) to O(n×m), often reducible",
                pseudoCode: @"# Bottom-up approach
def dp_solution(n):
    dp = [0] * (n + 1)
    dp[0] = base_case_0
    dp[1] = base_case_1
    
    for i in range(2, n + 1):
        dp[i] = recurrence_relation(dp[i-1], dp[i-2], ...)
    
    return dp[n]",
                triggerSignals: ToJson(new[] { "Count number of ways", "Minimum/maximum value", "Can you reach target?", "Longest/shortest sequence", "Make choices at each step", "Optimal decision sequence" }),
                commonMistakes: ToJson(new[] { "Wrong base case", "Incorrect recurrence relation", "Not considering all transitions", "Index out of bounds", "Not optimizing space when possible" }),
                resources: ToJson(new object[] {
                    new { title = "DP - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/5", type = "course" },
                    new { title = "DP Patterns", url = "https://leetcode.com/discuss/general-discussion/458695/dynamic-programming-patterns", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Backtracking Pattern
            Pattern.Create(
                name: "Backtracking",
                description: "Build solutions incrementally, abandoning (backtracking) paths that fail to satisfy constraints.",
                category: PatternCategory.Backtracking,
                whatItIs: @"Backtracking is a systematic way to explore all possible solutions by:
1. Making a choice
2. Recursively exploring with that choice
3. Undoing the choice (backtrack) and trying alternatives

It's essentially DFS on the decision tree with pruning.",
                whenToUse: @"Use Backtracking when:
- Need to generate all possible solutions
- Problem involves making sequences of decisions
- Need to find solutions satisfying constraints
- Exploring permutations, combinations, or subsets",
                whyItWorks: @"Backtracking works because:
1. Systematic exploration ensures completeness
2. Early pruning avoids exploring invalid paths
3. The undo step allows reusing state for different choices",
                commonUseCases: ToJson(new[] { "N-Queens", "Sudoku solver", "Generate permutations", "Combination sum", "Word search", "Palindrome partitioning" }),
                timeComplexity: "Often O(n!) or O(2^n) - exponential",
                spaceComplexity: "O(n) for recursion depth",
                pseudoCode: @"def backtrack(path, choices):
    if is_solution(path):
        result.append(path.copy())
        return
    
    for choice in choices:
        if is_valid(choice):
            path.append(choice)      # Make choice
            backtrack(path, remaining_choices)
            path.pop()               # Undo choice",
                triggerSignals: ToJson(new[] { "Generate all permutations/combinations", "Find all solutions", "Constraint satisfaction", "Explore decision tree", "Can't use DP (no overlapping subproblems)" }),
                commonMistakes: ToJson(new[] { "Forgetting to backtrack (undo choice)", "Not copying path when saving solution", "Inefficient pruning", "Wrong base case", "Duplicate solutions due to order" }),
                resources: ToJson(new object[] {
                    new { title = "Backtracking - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/6", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Heap/Priority Queue Pattern
            Pattern.Create(
                name: "Heap / Priority Queue",
                description: "Efficiently maintain and access the minimum or maximum element in a dynamic collection.",
                category: PatternCategory.Heap,
                whatItIs: @"A Heap is a complete binary tree where each parent is smaller (min-heap) or larger (max-heap) than its children. Operations:
- Insert: O(log n)
- Extract min/max: O(log n)
- Peek min/max: O(1)

Python's heapq is a min-heap; negate values for max-heap behavior.",
                whenToUse: @"Use Heap when:
- Need repeated access to min/max element
- Finding k largest/smallest elements
- Merge k sorted lists/streams
- Scheduling problems (process smallest/largest next)
- Median maintenance",
                whyItWorks: @"Heaps work because:
1. The heap property ensures min/max is always at root
2. Tree structure gives O(log n) insert and extract
3. Efficient for problems needing repeated min/max access",
                commonUseCases: ToJson(new[] { "Kth largest element", "Merge k sorted lists", "Find median from stream", "Task scheduler", "Top k frequent elements", "Meeting rooms" }),
                timeComplexity: "O(log n) per operation, O(n log k) for k elements",
                spaceComplexity: "O(n) or O(k)",
                pseudoCode: @"import heapq

# Min heap
heap = []
heapq.heappush(heap, value)
min_val = heapq.heappop(heap)

# Max heap (negate values)
heapq.heappush(heap, -value)
max_val = -heapq.heappop(heap)

# Heapify list
heapq.heapify(list)  # O(n)",
                triggerSignals: ToJson(new[] { "Kth largest/smallest", "Merge k sorted", "Continuously find min/max", "Top k elements", "Schedule by priority", "Median maintenance" }),
                commonMistakes: ToJson(new[] { "Using wrong heap type (min vs max)", "Forgetting to negate for max-heap in Python", "Not handling equal elements correctly", "Inefficient: sorting when heap suffices" }),
                resources: ToJson(new object[] {
                    new { title = "Heaps - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/3", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Greedy Pattern
            Pattern.Create(
                name: "Greedy",
                description: "Make locally optimal choices at each step, hoping to find a global optimum.",
                category: PatternCategory.Greedy,
                whatItIs: @"Greedy algorithms make the best choice at each step without considering future consequences. Key requirements:
1. Greedy choice property: local optimal leads to global optimal
2. Optimal substructure: optimal solution contains optimal sub-solutions

Not all problems have greedy solutions - proof or intuition needed.",
                whenToUse: @"Use Greedy when:
- Problem has greedy choice property
- Making locally optimal choice doesn't prevent global optimum
- Interval scheduling problems
- Huffman coding, MST algorithms
- Problems where sorting helps make decisions",
                whyItWorks: @"Greedy works because:
1. For certain problems, the locally optimal choice is part of the globally optimal solution
2. No need to explore all possibilities - one path suffices
3. Usually leads to efficient O(n log n) or O(n) solutions",
                commonUseCases: ToJson(new[] { "Activity selection", "Jump game", "Gas station", "Assign cookies", "Non-overlapping intervals", "Partition labels" }),
                timeComplexity: "Usually O(n log n) due to sorting, or O(n)",
                spaceComplexity: "Usually O(1) to O(n)",
                pseudoCode: @"def greedy_solution(items):
    # Often sort first
    items.sort(by=some_criteria)
    
    result = initial_value
    for item in items:
        if can_include(item):
            result = update_result(result, item)
    
    return result",
                triggerSignals: ToJson(new[] { "Maximize/minimize with constraints", "Interval problems", "Choose items with cost/value", "Local decision doesn't affect future options", "Sorting might help" }),
                commonMistakes: ToJson(new[] { "Applying greedy when DP is needed", "Wrong sorting criteria", "Not proving greedy works", "Missing edge cases", "Not considering ties properly" }),
                resources: ToJson(new object[] {
                    new { title = "Greedy Algorithms", url = "https://www.geeksforgeeks.org/greedy-algorithms/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Monotonic Stack Pattern
            Pattern.Create(
                name: "Monotonic Stack",
                description: "Maintain a stack with elements in sorted order to efficiently solve next greater/smaller element problems.",
                category: PatternCategory.MonotonicStack,
                whatItIs: @"A Monotonic Stack maintains elements in increasing or decreasing order. When a new element breaks the monotonic property, we pop elements and process them. This reveals relationships between elements (next greater, previous smaller, etc.).",
                whenToUse: @"Use Monotonic Stack when:
- Finding next/previous greater/smaller element
- Problems involving spans (stock span, largest rectangle)
- Tracking increasing/decreasing sequences
- Need O(1) amortized processing per element",
                whyItWorks: @"Monotonic Stack works because:
1. Each element is pushed and popped at most once: O(n) total
2. The monotonic property ensures when we pop, we've found the answer
3. Stack maintains a 'waiting list' of elements seeking their answer",
                commonUseCases: ToJson(new[] { "Next greater element", "Daily temperatures", "Largest rectangle in histogram", "Trapping rain water", "Stock span problem", "Sum of subarray minimums" }),
                timeComplexity: "O(n) - each element pushed/popped once",
                spaceComplexity: "O(n)",
                pseudoCode: @"def next_greater_elements(nums):
    n = len(nums)
    result = [-1] * n
    stack = []  # Stores indices
    
    for i in range(n):
        # Pop elements smaller than current
        while stack and nums[i] > nums[stack[-1]]:
            idx = stack.pop()
            result[idx] = nums[i]  # Found next greater
        stack.append(i)
    
    return result",
                triggerSignals: ToJson(new[] { "Next greater/smaller element", "Previous greater/smaller", "Span problems", "Histogram rectangle", "Temperature/stock problems", "Subarray min/max sums" }),
                commonMistakes: ToJson(new[] { "Wrong monotonic direction (increasing vs decreasing)", "Storing values instead of indices when indices needed", "Not handling remaining elements in stack", "Off-by-one errors" }),
                resources: ToJson(new object[] {
                    new { title = "Monotonic Stack - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/10", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Union-Find Pattern
            Pattern.Create(
                name: "Union-Find (Disjoint Set)",
                description: "Track connected components with near O(1) union and find operations using path compression and union by rank.",
                category: PatternCategory.UnionFind,
                whatItIs: @"Union-Find (Disjoint Set Union) maintains a collection of disjoint sets with operations:
- Find: Determine which set an element belongs to
- Union: Merge two sets

With path compression and union by rank, both operations are nearly O(1) (amortized).",
                whenToUse: @"Use Union-Find when:
- Tracking connected components dynamically
- Determining if elements are in same group
- Kruskal's MST algorithm
- Problems involving merging groups
- Cycle detection in undirected graphs",
                whyItWorks: @"Union-Find works because:
1. Path compression flattens the tree during find
2. Union by rank keeps trees balanced
3. Together they achieve O(α(n)) amortized time (inverse Ackermann, practically constant)",
                commonUseCases: ToJson(new[] { "Number of connected components", "Redundant connection", "Accounts merge", "Longest consecutive sequence", "Making a large island", "Satisfiability of equality equations" }),
                timeComplexity: "O(α(n)) ≈ O(1) per operation",
                spaceComplexity: "O(n)",
                pseudoCode: @"class UnionFind:
    def __init__(self, n):
        self.parent = list(range(n))
        self.rank = [0] * n
    
    def find(self, x):
        if self.parent[x] != x:
            self.parent[x] = self.find(self.parent[x])  # Path compression
        return self.parent[x]
    
    def union(self, x, y):
        px, py = self.find(x), self.find(y)
        if px == py:
            return False
        if self.rank[px] < self.rank[py]:
            px, py = py, px
        self.parent[py] = px
        if self.rank[px] == self.rank[py]:
            self.rank[px] += 1
        return True",
                triggerSignals: ToJson(new[] { "Connected components", "Group membership", "Merge groups", "Dynamic connectivity", "Cycle detection undirected graph" }),
                commonMistakes: ToJson(new[] { "Forgetting path compression", "Not using union by rank", "Incorrect parent initialization", "Not handling self-loops" }),
                resources: ToJson(new object[] {
                    new { title = "Union Find - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/9", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Trie Pattern  
            Pattern.Create(
                name: "Trie (Prefix Tree)",
                description: "Tree structure for efficient storage and retrieval of strings, enabling prefix-based operations.",
                category: PatternCategory.Trie,
                whatItIs: @"A Trie is a tree where each node represents a character. Paths from root to nodes form prefixes of stored strings. Each node can have up to 26 children (for lowercase English) and a flag marking end of word.",
                whenToUse: @"Use Trie when:
- Need prefix-based search (autocomplete)
- Storing and searching a dictionary of words
- Finding words with common prefix
- Word search in a grid
- Implementing spell checker",
                whyItWorks: @"Trie works because:
1. Common prefixes are stored once, saving space
2. Search time is O(m) where m is word length, independent of dictionary size
3. All words with a prefix can be found by traversing subtree",
                commonUseCases: ToJson(new[] { "Implement Trie", "Word search II", "Search suggestions system", "Design add and search words", "Replace words", "Palindrome pairs" }),
                timeComplexity: "O(m) per operation where m is word length",
                spaceComplexity: "O(total characters in all words)",
                pseudoCode: @"class TrieNode:
    def __init__(self):
        self.children = {}
        self.is_end = False

class Trie:
    def __init__(self):
        self.root = TrieNode()
    
    def insert(self, word):
        node = self.root
        for char in word:
            if char not in node.children:
                node.children[char] = TrieNode()
            node = node.children[char]
        node.is_end = True
    
    def search(self, word):
        node = self._traverse(word)
        return node is not None and node.is_end
    
    def starts_with(self, prefix):
        return self._traverse(prefix) is not None",
                triggerSignals: ToJson(new[] { "Prefix search", "Autocomplete", "Word dictionary", "Multiple string search", "Common prefix operations" }),
                commonMistakes: ToJson(new[] { "Forgetting is_end flag", "Not handling empty strings", "Memory inefficiency with sparse tries", "Case sensitivity issues" }),
                resources: ToJson(new object[] {
                    new { title = "Trie - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/11", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Intervals Pattern
            Pattern.Create(
                name: "Intervals",
                description: "Techniques for handling overlapping intervals: merging, finding gaps, and scheduling.",
                category: PatternCategory.Intervals,
                whatItIs: @"Interval problems involve ranges [start, end]. Common operations:
- Merge overlapping intervals
- Find if intervals overlap
- Insert interval
- Find gaps between intervals

Key technique: Sort by start (or end) time, then process linearly.",
                whenToUse: @"Use Interval techniques when:
- Dealing with time ranges or segments
- Meeting room scheduling
- Merging or splitting ranges
- Finding overlaps or conflicts
- Calendar/booking problems",
                whyItWorks: @"Interval techniques work because:
1. Sorting allows linear processing
2. After sorting, overlaps can be detected by comparing adjacent intervals
3. Greedy choices (by end time) often yield optimal solutions",
                commonUseCases: ToJson(new[] { "Merge intervals", "Insert interval", "Meeting rooms", "Non-overlapping intervals", "Minimum number of arrows", "Employee free time" }),
                timeComplexity: "O(n log n) due to sorting",
                spaceComplexity: "O(n) for result, O(1) extra",
                pseudoCode: @"def merge_intervals(intervals):
    intervals.sort(key=lambda x: x[0])
    result = [intervals[0]]
    
    for start, end in intervals[1:]:
        if start <= result[-1][1]:  # Overlaps
            result[-1][1] = max(result[-1][1], end)
        else:
            result.append([start, end])
    
    return result",
                triggerSignals: ToJson(new[] { "Time ranges", "Overlapping intervals", "Merge ranges", "Meeting scheduling", "Booking conflicts", "Minimum operations on ranges" }),
                commonMistakes: ToJson(new[] { "Wrong sort key", "Not handling touching intervals correctly", "Off-by-one with inclusive/exclusive bounds", "Not merging all overlaps" }),
                resources: ToJson(new object[] {
                    new { title = "Intervals - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/12", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            )
        ];
    }

    private static List<DataStructure> CreateDataStructures()
    {
        return
        [
            // Array
            DataStructure.Create(
                name: "Array",
                description: "Contiguous block of memory storing elements of the same type, accessed by index.",
                category: DataStructureCategory.Linear,
                whatItIs: @"An array is a fundamental data structure that stores elements in contiguous memory locations. Each element is accessed by its index (position) in constant time. Arrays have a fixed size in many languages, though dynamic arrays (like Python lists) can grow.",
                operations: ToJson(new object[] {
                    new { name = "Access by index", timeComplexity = "O(1)", description = "Get element at position i" },
                    new { name = "Update by index", timeComplexity = "O(1)", description = "Set element at position i" },
                    new { name = "Insert at end", timeComplexity = "O(1)*", description = "Amortized O(1) for dynamic arrays" },
                    new { name = "Insert at position", timeComplexity = "O(n)", description = "Shift elements right" },
                    new { name = "Delete at position", timeComplexity = "O(n)", description = "Shift elements left" },
                    new { name = "Search (unsorted)", timeComplexity = "O(n)", description = "Linear scan" },
                    new { name = "Search (sorted)", timeComplexity = "O(log n)", description = "Binary search" }
                }),
                whenToUse: @"Use arrays when:
- Need fast random access by index
- Data size is known or grows mainly at the end
- Memory locality matters for performance
- Implementing other data structures",
                tradeoffs: @"Pros:
- O(1) random access
- Memory efficient (no pointers)
- Cache-friendly (contiguous memory)

Cons:
- Fixed size (static arrays)
- Expensive insertion/deletion in middle
- May waste space if not full",
                commonUseCases: ToJson(new[] { "Storing collections", "Implementing stacks/queues", "Matrix operations", "Buffers", "Hash table backing" }),
                implementation: @"# Python list (dynamic array)
arr = [1, 2, 3, 4, 5]
arr[0]          # Access: O(1)
arr[2] = 10     # Update: O(1)
arr.append(6)   # Insert end: O(1) amortized
arr.pop()       # Delete end: O(1)
arr.insert(1, 9) # Insert at index: O(n)",
                resources: ToJson(new object[] {
                    new { title = "Arrays 101 - LeetCode", url = "https://leetcode.com/explore/learn/card/fun-with-arrays/", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Linked List
            DataStructure.Create(
                name: "Linked List",
                description: "Linear collection where each element (node) contains data and reference to the next node.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A linked list is a linear data structure where elements (nodes) are not stored in contiguous memory. Each node contains data and a pointer to the next node. Variants include doubly linked lists (pointers to both next and previous) and circular linked lists.",
                operations: ToJson(new object[] {
                    new { name = "Access by index", timeComplexity = "O(n)", description = "Must traverse from head" },
                    new { name = "Insert at head", timeComplexity = "O(1)", description = "Update head pointer" },
                    new { name = "Insert at tail", timeComplexity = "O(1)*", description = "O(1) with tail pointer, O(n) without" },
                    new { name = "Insert after node", timeComplexity = "O(1)", description = "If node reference given" },
                    new { name = "Delete node", timeComplexity = "O(1)", description = "If node reference given" },
                    new { name = "Search", timeComplexity = "O(n)", description = "Linear traversal" }
                }),
                whenToUse: @"Use linked lists when:
- Frequent insertions/deletions at any position
- Don't need random access
- Don't know size in advance
- Implementing stacks, queues, or LRU cache",
                tradeoffs: @"Pros:
- O(1) insertion/deletion (with reference)
- Dynamic size
- No wasted space

Cons:
- O(n) random access
- Extra memory for pointers
- Poor cache locality",
                commonUseCases: ToJson(new[] { "LRU Cache", "Undo functionality", "Music playlist", "Browser history", "Polynomial representation" }),
                implementation: @"class ListNode:
    def __init__(self, val=0, next=None):
        self.val = val
        self.next = next

# Insert at head
def insert_head(head, val):
    new_node = ListNode(val)
    new_node.next = head
    return new_node

# Delete node (given prev)
def delete_after(prev):
    if prev and prev.next:
        prev.next = prev.next.next",
                resources: ToJson(new object[] {
                    new { title = "Linked List - LeetCode", url = "https://leetcode.com/explore/learn/card/linked-list/", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Stack
            DataStructure.Create(
                name: "Stack",
                description: "LIFO (Last-In-First-Out) collection supporting push and pop operations.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A stack is a linear data structure following Last-In-First-Out (LIFO) principle. The last element added is the first one removed. Think of a stack of plates - you add and remove from the top only.",
                operations: ToJson(new object[] {
                    new { name = "Push", timeComplexity = "O(1)", description = "Add element to top" },
                    new { name = "Pop", timeComplexity = "O(1)", description = "Remove and return top element" },
                    new { name = "Peek/Top", timeComplexity = "O(1)", description = "View top element without removing" },
                    new { name = "isEmpty", timeComplexity = "O(1)", description = "Check if stack is empty" },
                    new { name = "Size", timeComplexity = "O(1)", description = "Number of elements" }
                }),
                whenToUse: @"Use stacks when:
- Need LIFO ordering
- Tracking nested structures (parentheses, HTML tags)
- Undo operations
- DFS traversal
- Expression evaluation",
                tradeoffs: @"Pros:
- Simple interface
- All operations O(1)
- Perfect for LIFO patterns

Cons:
- No random access
- Can only access top element
- Fixed ordering",
                commonUseCases: ToJson(new[] { "Valid parentheses", "Reverse Polish notation", "Function call stack", "Browser back button", "Undo/Redo" }),
                implementation: @"# Using list as stack in Python
stack = []
stack.append(1)     # Push
stack.append(2)
top = stack.pop()   # Pop: returns 2
peek = stack[-1]    # Peek: returns 1
is_empty = len(stack) == 0",
                resources: ToJson(new object[] {
                    new { title = "Stack Problems", url = "https://leetcode.com/tag/stack/", type = "practice" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Queue
            DataStructure.Create(
                name: "Queue",
                description: "FIFO (First-In-First-Out) collection supporting enqueue and dequeue operations.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A queue is a linear data structure following First-In-First-Out (FIFO) principle. The first element added is the first one removed. Think of a line at a store - first person in line is served first.",
                operations: ToJson(new object[] {
                    new { name = "Enqueue", timeComplexity = "O(1)", description = "Add element to back" },
                    new { name = "Dequeue", timeComplexity = "O(1)", description = "Remove and return front element" },
                    new { name = "Front/Peek", timeComplexity = "O(1)", description = "View front element" },
                    new { name = "isEmpty", timeComplexity = "O(1)", description = "Check if queue is empty" },
                    new { name = "Size", timeComplexity = "O(1)", description = "Number of elements" }
                }),
                whenToUse: @"Use queues when:
- Need FIFO ordering
- BFS traversal
- Task scheduling
- Buffering data streams
- Producer-consumer problems",
                tradeoffs: @"Pros:
- Simple interface
- All operations O(1)
- Natural for FIFO patterns

Cons:
- No random access
- Can only access front element",
                commonUseCases: ToJson(new[] { "BFS traversal", "Level-order tree traversal", "Task scheduling", "Print queue", "Message queues" }),
                implementation: @"from collections import deque

queue = deque()
queue.append(1)      # Enqueue
queue.append(2)
front = queue.popleft()  # Dequeue: returns 1
peek = queue[0]      # Peek front
is_empty = len(queue) == 0",
                resources: ToJson(new object[] {
                    new { title = "Queue Problems", url = "https://leetcode.com/tag/queue/", type = "practice" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Hash Table
            DataStructure.Create(
                name: "Hash Table (Dictionary)",
                description: "Key-value store with average O(1) lookup using hash function.",
                category: DataStructureCategory.HashBased,
                whatItIs: @"A hash table (dictionary/map) stores key-value pairs using a hash function to compute an index. Collisions are handled via chaining (linked lists) or open addressing. Provides near-constant time operations on average.",
                operations: ToJson(new object[] {
                    new { name = "Insert", timeComplexity = "O(1)*", description = "Average case, O(n) worst with collisions" },
                    new { name = "Delete", timeComplexity = "O(1)*", description = "Average case" },
                    new { name = "Lookup", timeComplexity = "O(1)*", description = "Average case" },
                    new { name = "Contains Key", timeComplexity = "O(1)*", description = "Average case" },
                    new { name = "Iterate", timeComplexity = "O(n)", description = "Visit all key-value pairs" }
                }),
                whenToUse: @"Use hash tables when:
- Need fast key-based lookup
- Counting frequencies
- Caching/memoization
- Detecting duplicates
- Grouping by key",
                tradeoffs: @"Pros:
- O(1) average operations
- Flexible keys
- Natural for counting/grouping

Cons:
- No ordering
- Worst case O(n) with collisions
- Memory overhead for load factor",
                commonUseCases: ToJson(new[] { "Two Sum", "Frequency counting", "Caching", "Anagram grouping", "LRU Cache", "Symbol tables" }),
                implementation: @"# Python dict
d = {}
d['key'] = 'value'      # Insert
val = d['key']          # Lookup
del d['key']            # Delete
exists = 'key' in d     # Contains
d.get('key', default)   # Safe lookup

# Counting
from collections import Counter
counts = Counter([1,1,2,3,3,3])",
                resources: ToJson(new object[] {
                    new { title = "Hash Table - LeetCode", url = "https://leetcode.com/explore/learn/card/hash-table/", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Hash Set
            DataStructure.Create(
                name: "Hash Set",
                description: "Collection of unique elements with O(1) average membership testing.",
                category: DataStructureCategory.HashBased,
                whatItIs: @"A hash set is a collection that stores unique elements with no duplicates. Uses hashing for fast membership testing. Elements must be hashable (immutable in Python).",
                operations: ToJson(new object[] {
                    new { name = "Add", timeComplexity = "O(1)*", description = "Add element (no-op if exists)" },
                    new { name = "Remove", timeComplexity = "O(1)*", description = "Remove element" },
                    new { name = "Contains", timeComplexity = "O(1)*", description = "Check membership" },
                    new { name = "Union", timeComplexity = "O(n+m)", description = "Combine two sets" },
                    new { name = "Intersection", timeComplexity = "O(min(n,m))", description = "Common elements" }
                }),
                whenToUse: @"Use hash sets when:
- Need unique elements only
- Fast membership testing
- Detecting duplicates
- Set operations (union, intersection)
- Tracking visited states",
                tradeoffs: @"Pros:
- O(1) membership test
- Automatic duplicate handling
- Set operations

Cons:
- No ordering
- Elements must be hashable
- Memory overhead",
                commonUseCases: ToJson(new[] { "Contains Duplicate", "Longest consecutive sequence", "Visited tracking", "Finding unique elements", "Set intersection/union" }),
                implementation: @"# Python set
s = set()
s.add(1)           # Add
s.add(2)
s.remove(1)        # Remove (raises if missing)
s.discard(1)       # Remove (no error if missing)
exists = 2 in s    # Contains: O(1)

# Set from list (deduplicates)
unique = set([1, 2, 2, 3])  # {1, 2, 3}",
                resources: ToJson(new object[] {
                    new { title = "Set Problems", url = "https://leetcode.com/tag/hash-table/", type = "practice" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Binary Search Tree
            DataStructure.Create(
                name: "Binary Search Tree (BST)",
                description: "Tree where left children are smaller and right children are larger than parent.",
                category: DataStructureCategory.Tree,
                whatItIs: @"A Binary Search Tree is a tree where each node has at most two children, and for every node: all values in left subtree are smaller, all values in right subtree are larger. This property enables efficient searching.",
                operations: ToJson(new object[] {
                    new { name = "Search", timeComplexity = "O(h)*", description = "O(log n) balanced, O(n) worst" },
                    new { name = "Insert", timeComplexity = "O(h)*", description = "O(log n) balanced, O(n) worst" },
                    new { name = "Delete", timeComplexity = "O(h)*", description = "O(log n) balanced, O(n) worst" },
                    new { name = "Min/Max", timeComplexity = "O(h)", description = "Leftmost/rightmost node" },
                    new { name = "Inorder traversal", timeComplexity = "O(n)", description = "Visits nodes in sorted order" }
                }),
                whenToUse: @"Use BST when:
- Need ordered data with search/insert/delete
- Finding min/max efficiently
- Range queries
- Predecessor/successor queries",
                tradeoffs: @"Pros:
- Ordered data
- O(log n) operations when balanced
- Inorder gives sorted sequence

Cons:
- Can degenerate to O(n)
- More complex than hash table
- Needs balancing for guarantees",
                commonUseCases: ToJson(new[] { "Validate BST", "Kth smallest element", "LCA in BST", "BST iterator", "Range sum of BST" }),
                implementation: @"class TreeNode:
    def __init__(self, val=0, left=None, right=None):
        self.val = val
        self.left = left
        self.right = right

def search(root, target):
    if not root:
        return None
    if root.val == target:
        return root
    elif target < root.val:
        return search(root.left, target)
    else:
        return search(root.right, target)",
                resources: ToJson(new object[] {
                    new { title = "BST - LeetCode", url = "https://leetcode.com/explore/learn/card/introduction-to-data-structure-binary-search-tree/", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Heap
            DataStructure.Create(
                name: "Heap (Priority Queue)",
                description: "Complete binary tree where parent is always smaller (min-heap) or larger (max-heap) than children.",
                category: DataStructureCategory.Heap,
                whatItIs: @"A heap is a complete binary tree satisfying the heap property: in a min-heap, every parent is ≤ its children; in a max-heap, every parent is ≥ its children. The root is always the min/max element. Usually implemented as an array.",
                operations: ToJson(new object[] {
                    new { name = "Insert", timeComplexity = "O(log n)", description = "Add and bubble up" },
                    new { name = "Extract min/max", timeComplexity = "O(log n)", description = "Remove root and heapify" },
                    new { name = "Peek min/max", timeComplexity = "O(1)", description = "View root" },
                    new { name = "Heapify array", timeComplexity = "O(n)", description = "Build heap from array" },
                    new { name = "Update priority", timeComplexity = "O(log n)", description = "Change value and reheapify" }
                }),
                whenToUse: @"Use heaps when:
- Need quick access to min or max
- Implementing priority queue
- K largest/smallest problems
- Merge k sorted lists
- Median finding",
                tradeoffs: @"Pros:
- O(1) access to min/max
- O(log n) insert/extract
- Space efficient (array)

Cons:
- Only access to min OR max
- Not sorted
- No efficient search",
                commonUseCases: ToJson(new[] { "Kth largest element", "Merge k sorted lists", "Find median", "Task scheduler", "Top k frequent" }),
                implementation: @"import heapq

# Min heap (default)
heap = []
heapq.heappush(heap, 3)
heapq.heappush(heap, 1)
min_val = heapq.heappop(heap)  # Returns 1

# Max heap (negate values)
max_heap = []
heapq.heappush(max_heap, -3)
max_val = -heapq.heappop(max_heap)  # Returns 3

# Heapify existing list
arr = [3, 1, 4, 1, 5]
heapq.heapify(arr)  # O(n)",
                resources: ToJson(new object[] {
                    new { title = "Heap - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/3", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Trie
            DataStructure.Create(
                name: "Trie (Prefix Tree)",
                description: "Tree structure for efficient string storage and prefix-based retrieval.",
                category: DataStructureCategory.Tree,
                whatItIs: @"A Trie is a tree-like data structure for storing strings where each node represents a character. The path from root to a node spells out a prefix. Enables fast prefix operations and autocomplete functionality.",
                operations: ToJson(new object[] {
                    new { name = "Insert word", timeComplexity = "O(m)", description = "m = word length" },
                    new { name = "Search word", timeComplexity = "O(m)", description = "Check if word exists" },
                    new { name = "Search prefix", timeComplexity = "O(m)", description = "Check if prefix exists" },
                    new { name = "Delete word", timeComplexity = "O(m)", description = "Remove word" },
                    new { name = "Autocomplete", timeComplexity = "O(m + k)", description = "m=prefix, k=results" }
                }),
                whenToUse: @"Use tries when:
- Prefix-based searches
- Autocomplete/typeahead
- Spell checking
- IP routing tables
- Word search in grid",
                tradeoffs: @"Pros:
- O(m) operations, independent of dictionary size
- Efficient prefix matching
- Natural for string problems

Cons:
- Memory intensive
- Slower than hash for exact lookup
- Complex implementation",
                commonUseCases: ToJson(new[] { "Implement Trie", "Word Search II", "Autocomplete", "Spell checker", "Replace words" }),
                implementation: @"class TrieNode:
    def __init__(self):
        self.children = {}
        self.is_end = False

class Trie:
    def __init__(self):
        self.root = TrieNode()
    
    def insert(self, word):
        node = self.root
        for c in word:
            if c not in node.children:
                node.children[c] = TrieNode()
            node = node.children[c]
        node.is_end = True",
                resources: ToJson(new object[] {
                    new { title = "Trie - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/11", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Graph (Adjacency List)
            DataStructure.Create(
                name: "Graph (Adjacency List)",
                description: "Collection of vertices connected by edges, represented as lists of neighbors.",
                category: DataStructureCategory.Graph,
                whatItIs: @"A graph consists of vertices (nodes) and edges connecting them. The adjacency list representation stores, for each vertex, a list of its neighbors. Efficient for sparse graphs. Can be directed or undirected, weighted or unweighted.",
                operations: ToJson(new object[] {
                    new { name = "Add vertex", timeComplexity = "O(1)", description = "Create empty neighbor list" },
                    new { name = "Add edge", timeComplexity = "O(1)", description = "Add to adjacency list" },
                    new { name = "Remove edge", timeComplexity = "O(degree)", description = "Search and remove from list" },
                    new { name = "Check edge", timeComplexity = "O(degree)", description = "Search neighbor list" },
                    new { name = "Get neighbors", timeComplexity = "O(1)", description = "Return adjacency list" },
                    new { name = "DFS/BFS", timeComplexity = "O(V+E)", description = "Visit all vertices and edges" }
                }),
                whenToUse: @"Use adjacency list when:
- Graph is sparse (E << V²)
- Need to iterate over neighbors
- Memory is a concern
- Most graph algorithms",
                tradeoffs: @"Pros:
- Space efficient for sparse graphs: O(V+E)
- Fast neighbor iteration
- Easy to implement

Cons:
- O(degree) edge lookup
- Slower than matrix for dense graphs",
                commonUseCases: ToJson(new[] { "Social networks", "Road networks", "Course prerequisites", "Web crawling", "Recommendation systems" }),
                implementation: @"from collections import defaultdict

# Adjacency list representation
graph = defaultdict(list)

# Add edges (undirected)
def add_edge(u, v):
    graph[u].append(v)
    graph[v].append(u)

# Get neighbors
neighbors = graph[node]

# Build from edge list
edges = [[0,1], [1,2], [2,0]]
for u, v in edges:
    add_edge(u, v)",
                resources: ToJson(new object[] {
                    new { title = "Graph - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/7", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Deque
            DataStructure.Create(
                name: "Deque (Double-ended Queue)",
                description: "Queue allowing insertion and deletion at both ends in O(1).",
                category: DataStructureCategory.Linear,
                whatItIs: @"A deque (double-ended queue) supports efficient insertion and deletion at both front and back. It combines the capabilities of both stack and queue. Python's collections.deque is implemented as a doubly-linked list.",
                operations: ToJson(new object[] {
                    new { name = "Append (right)", timeComplexity = "O(1)", description = "Add to back" },
                    new { name = "Append left", timeComplexity = "O(1)", description = "Add to front" },
                    new { name = "Pop (right)", timeComplexity = "O(1)", description = "Remove from back" },
                    new { name = "Pop left", timeComplexity = "O(1)", description = "Remove from front" },
                    new { name = "Access ends", timeComplexity = "O(1)", description = "Peek front/back" },
                    new { name = "Access by index", timeComplexity = "O(n)", description = "Random access slow" }
                }),
                whenToUse: @"Use deque when:
- Need O(1) operations at both ends
- Implementing sliding window
- BFS (more efficient than list)
- Need both stack and queue operations",
                tradeoffs: @"Pros:
- O(1) at both ends
- More flexible than stack or queue
- Good for sliding window

Cons:
- O(n) random access
- More memory than array
- Slightly complex API",
                commonUseCases: ToJson(new[] { "Sliding window maximum", "BFS implementation", "Palindrome checker", "Task scheduling", "Recent calls" }),
                implementation: @"from collections import deque

dq = deque()
dq.append(1)        # Add to right
dq.appendleft(0)    # Add to left
right = dq.pop()    # Remove from right
left = dq.popleft() # Remove from left

# With max size (auto-removes from other end)
dq = deque(maxlen=3)",
                resources: ToJson(new object[] {
                    new { title = "Deque Documentation", url = "https://docs.python.org/3/library/collections.html#collections.deque", type = "documentation" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Union-Find
            DataStructure.Create(
                name: "Union-Find (Disjoint Set)",
                description: "Data structure for tracking disjoint sets with near O(1) union and find operations.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"Union-Find (Disjoint Set Union) maintains a collection of non-overlapping sets. Supports finding which set an element belongs to and merging sets. With path compression and union by rank, operations are nearly O(1) amortized.",
                operations: ToJson(new object[] {
                    new { name = "Find", timeComplexity = "O(α(n))", description = "Find set representative (nearly O(1))" },
                    new { name = "Union", timeComplexity = "O(α(n))", description = "Merge two sets" },
                    new { name = "Connected", timeComplexity = "O(α(n))", description = "Check if same set" },
                    new { name = "Count sets", timeComplexity = "O(1)", description = "Number of disjoint sets" }
                }),
                whenToUse: @"Use Union-Find when:
- Tracking connected components
- Dynamic connectivity
- Kruskal's MST
- Detecting cycles in undirected graph
- Grouping equivalent items",
                tradeoffs: @"Pros:
- Nearly O(1) operations
- Efficient for connectivity queries
- Simple to implement

Cons:
- Only tracks connectivity, not paths
- Cannot efficiently split sets
- Amortized not worst-case",
                commonUseCases: ToJson(new[] { "Number of connected components", "Redundant connection", "Accounts merge", "Making a large island", "Graph valid tree" }),
                implementation: @"class UnionFind:
    def __init__(self, n):
        self.parent = list(range(n))
        self.rank = [0] * n
        self.count = n  # Number of components
    
    def find(self, x):
        if self.parent[x] != x:
            self.parent[x] = self.find(self.parent[x])
        return self.parent[x]
    
    def union(self, x, y):
        px, py = self.find(x), self.find(y)
        if px == py:
            return False
        if self.rank[px] < self.rank[py]:
            px, py = py, px
        self.parent[py] = px
        if self.rank[px] == self.rank[py]:
            self.rank[px] += 1
        self.count -= 1
        return True",
                resources: ToJson(new object[] {
                    new { title = "Union Find - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/9", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            )
        ];
    }
}
