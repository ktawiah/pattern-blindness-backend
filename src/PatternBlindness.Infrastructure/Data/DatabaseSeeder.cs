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
        await SeedDataStructuresAsync(context);
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

        foreach (var newDs in dataStructures)
        {
            var existingDs = await context.DataStructures
                .FirstOrDefaultAsync(ds => ds.Name == newDs.Name);

            if (existingDs == null)
            {
                await context.DataStructures.AddAsync(newDs);
            }
            else
            {
                existingDs.Update(
                    name: newDs.Name,
                    description: newDs.Description,
                    whatItIs: newDs.WhatItIs,
                    operations: newDs.Operations,
                    whenToUse: newDs.WhenToUse,
                    tradeoffs: newDs.Tradeoffs,
                    commonUseCases: newDs.CommonUseCases,
                    implementation: newDs.Implementation,
                    resources: newDs.Resources,
                    relatedStructureIds: newDs.RelatedStructureIds
                );
            }
        }

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
            ),

            // Divide and Conquer Pattern
            Pattern.Create(
                name: "Divide and Conquer",
                description: "Break problem into smaller subproblems, solve recursively, and combine results.",
                category: PatternCategory.DivideAndConquer,
                whatItIs: @"Divide and Conquer is a paradigm that breaks a problem into smaller independent subproblems, solves them recursively, and combines the results. Unlike DP, subproblems don't overlap.

Three steps:
1. Divide: Split into smaller subproblems
2. Conquer: Solve subproblems recursively
3. Combine: Merge subproblem solutions",
                whenToUse: @"Use Divide and Conquer when:
- Problem can be broken into independent subproblems
- Subproblems are similar to the original problem
- Solutions can be combined efficiently
- Classic examples: sorting, searching, matrix multiplication",
                whyItWorks: @"Divide and Conquer works because:
1. Smaller problems are easier to solve
2. Recursion naturally handles the division
3. Combining is often O(n) leading to O(n log n) total
4. Can be parallelized since subproblems are independent",
                commonUseCases: ToJson(new[] { "Merge Sort", "Quick Sort", "Binary Search", "Maximum subarray (Kadane)", "Closest pair of points", "Strassen's matrix multiplication", "Karatsuba multiplication" }),
                timeComplexity: "Varies: O(n log n) for sorting, O(log n) for search",
                spaceComplexity: "O(log n) to O(n) for recursion stack",
                pseudoCode: @"def divide_and_conquer(problem):
    # Base case
    if is_base_case(problem):
        return solve_directly(problem)

    # Divide
    subproblems = divide(problem)

    # Conquer
    subsolutions = [divide_and_conquer(sub) for sub in subproblems]

    # Combine
    return combine(subsolutions)

# Merge Sort example
def merge_sort(arr):
    if len(arr) <= 1:
        return arr
    mid = len(arr) // 2
    left = merge_sort(arr[:mid])
    right = merge_sort(arr[mid:])
    return merge(left, right)",
                triggerSignals: ToJson(new[] { "Problem can be halved", "Subproblems are independent", "Sorting algorithms", "Matrix operations", "Geometric algorithms" }),
                commonMistakes: ToJson(new[] { "Wrong base case", "Inefficient combine step", "Not handling odd/even splits", "Stack overflow on deep recursion", "Using when DP is needed (overlapping subproblems)" }),
                resources: ToJson(new object[] {
                    new { title = "Divide and Conquer - Khan Academy", url = "https://www.khanacademy.org/computing/computer-science/algorithms/merge-sort/a/divide-and-conquer-algorithms", type = "article" },
                    new { title = "Divide and Conquer - GeeksforGeeks", url = "https://www.geeksforgeeks.org/divide-and-conquer/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Topological Sort Pattern
            Pattern.Create(
                name: "Topological Sort",
                description: "Linear ordering of vertices in a DAG where every directed edge u→v has u before v.",
                category: PatternCategory.Graph,
                whatItIs: @"Topological Sort produces a linear ordering of vertices in a Directed Acyclic Graph (DAG) such that for every edge (u, v), vertex u comes before v. Two approaches:
1. Kahn's Algorithm (BFS with indegree)
2. DFS with post-order reversal

Only possible for DAGs - existence of topological order proves no cycles.",
                whenToUse: @"Use Topological Sort when:
- Processing dependencies in order
- Build systems (compile order)
- Course prerequisites
- Task scheduling with dependencies
- Detecting cycles in directed graphs",
                whyItWorks: @"Topological Sort works because:
1. In a DAG, there's always a vertex with no incoming edges
2. Removing it and repeating gives valid ordering
3. DFS post-order naturally respects dependencies",
                commonUseCases: ToJson(new[] { "Course Schedule", "Alien Dictionary", "Build order", "Task scheduling", "Recipe steps", "Package dependencies" }),
                timeComplexity: "O(V + E)",
                spaceComplexity: "O(V) for result and auxiliary structures",
                pseudoCode: @"# Kahn's Algorithm (BFS)
def topological_sort(graph, num_nodes):
    indegree = [0] * num_nodes
    for u in graph:
        for v in graph[u]:
            indegree[v] += 1

    queue = deque([i for i in range(num_nodes) if indegree[i] == 0])
    result = []

    while queue:
        node = queue.popleft()
        result.append(node)
        for neighbor in graph[node]:
            indegree[neighbor] -= 1
            if indegree[neighbor] == 0:
                queue.append(neighbor)

    return result if len(result) == num_nodes else []  # Empty if cycle",
                triggerSignals: ToJson(new[] { "Dependencies/prerequisites", "Build order", "Course schedule", "DAG processing", "Cycle detection directed graph" }),
                commonMistakes: ToJson(new[] { "Forgetting to check for cycles", "Wrong indegree calculation", "Not handling disconnected components", "Confusing with DFS/BFS traversal" }),
                resources: ToJson(new object[] {
                    new { title = "Topological Sort - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/14", type = "course" },
                    new { title = "Kahn's Algorithm", url = "https://www.geeksforgeeks.org/topological-sorting-indegree-based-solution/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Bit Manipulation Pattern
            Pattern.Create(
                name: "Bit Manipulation",
                description: "Use bitwise operations to solve problems efficiently with constant space.",
                category: PatternCategory.BitManipulation,
                whatItIs: @"Bit manipulation uses bitwise operators (AND, OR, XOR, NOT, shifts) to solve problems at the bit level. Key operations:
- x & (x-1): Clear lowest set bit
- x & (-x): Get lowest set bit
- x ^ x = 0: XOR of same numbers is 0
- x ^ 0 = x: XOR with 0 preserves value

Enables O(1) space solutions for certain problems.",
                whenToUse: @"Use Bit Manipulation when:
- Finding single/unique numbers
- Power of 2 checks
- Counting bits
- Generating subsets
- Space optimization (bit vectors)",
                whyItWorks: @"Bit Manipulation works because:
1. XOR has properties: x^x=0, x^0=x, associative
2. Numbers can encode multiple boolean states
3. Bitwise operations are O(1) CPU operations
4. Bits can represent sets efficiently",
                commonUseCases: ToJson(new[] { "Single Number", "Number of 1 Bits", "Power of Two", "Counting Bits", "Subsets generation", "Missing Number", "Reverse Bits" }),
                timeComplexity: "O(1) to O(n) depending on problem",
                spaceComplexity: "Often O(1)",
                pseudoCode: @"# Common bit operations
n & (n - 1)      # Clear lowest set bit
n & (-n)         # Isolate lowest set bit
n | (1 << i)     # Set bit at position i
n & ~(1 << i)    # Clear bit at position i
(n >> i) & 1     # Check bit at position i

# Single Number (XOR all elements)
def single_number(nums):
    result = 0
    for num in nums:
        result ^= num
    return result

# Count set bits
def count_bits(n):
    count = 0
    while n:
        n &= (n - 1)  # Clear lowest set bit
        count += 1
    return count",
                triggerSignals: ToJson(new[] { "Find unique/single number", "Power of 2", "Binary representation", "Subsets/combinations", "O(1) space requirement", "Toggle states" }),
                commonMistakes: ToJson(new[] { "Sign bit issues with negative numbers", "Wrong shift direction", "Overflow in bit shifts", "Not handling zero case" }),
                resources: ToJson(new object[] {
                    new { title = "Bit Manipulation - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/15", type = "course" },
                    new { title = "Bit Manipulation Tutorial", url = "https://leetcode.com/discuss/general-discussion/1073221/all-about-bitwise-operations-beginner-intermediate", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Prefix Sum Pattern
            Pattern.Create(
                name: "Prefix Sum",
                description: "Precompute cumulative sums to answer range sum queries in O(1).",
                category: PatternCategory.PrefixSum,
                whatItIs: @"Prefix Sum creates an array where each element is the sum of all previous elements plus itself. This allows O(1) range sum queries:
- prefix[j] - prefix[i-1] = sum(arr[i:j+1])

Can be extended to 2D (prefix sum matrix) and other operations (product, XOR).",
                whenToUse: @"Use Prefix Sum when:
- Multiple range sum queries
- Finding subarrays with target sum
- Cumulative statistics needed
- Range operations on arrays
- 2D matrix region queries",
                whyItWorks: @"Prefix Sum works because:
1. Precomputation amortizes over many queries
2. Range sum = difference of two prefix sums
3. Avoids O(n) computation per query",
                commonUseCases: ToJson(new[] { "Range Sum Query", "Subarray Sum Equals K", "Contiguous Array", "Product of Array Except Self", "2D Range Sum", "Running sum" }),
                timeComplexity: "O(n) precompute, O(1) query",
                spaceComplexity: "O(n) for prefix array",
                pseudoCode: @"# Build prefix sum
def build_prefix(arr):
    prefix = [0] * (len(arr) + 1)
    for i in range(len(arr)):
        prefix[i + 1] = prefix[i] + arr[i]
    return prefix

# Range sum query [i, j] inclusive
def range_sum(prefix, i, j):
    return prefix[j + 1] - prefix[i]

# Subarray sum equals k
def subarray_sum(nums, k):
    count = 0
    prefix_sum = 0
    seen = {0: 1}

    for num in nums:
        prefix_sum += num
        if prefix_sum - k in seen:
            count += seen[prefix_sum - k]
        seen[prefix_sum] = seen.get(prefix_sum, 0) + 1

    return count",
                triggerSignals: ToJson(new[] { "Range sum queries", "Subarray with target sum", "Cumulative operations", "Multiple queries on same array", "2D matrix region sums" }),
                commonMistakes: ToJson(new[] { "Off-by-one in prefix indexing", "Not handling empty ranges", "Integer overflow with large sums", "Forgetting prefix[0] = 0" }),
                resources: ToJson(new object[] {
                    new { title = "Prefix Sum - LeetCode", url = "https://leetcode.com/problems/range-sum-query-immutable/", type = "problem" },
                    new { title = "Prefix Sum Tutorial", url = "https://www.geeksforgeeks.org/prefix-sum-array-implementation-applications-competitive-programming/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Fast and Slow Pointers Pattern
            Pattern.Create(
                name: "Fast and Slow Pointers (Floyd's)",
                description: "Use two pointers moving at different speeds to detect cycles and find middle elements.",
                category: PatternCategory.TwoPointers,
                whatItIs: @"Fast and Slow Pointers (Floyd's Tortoise and Hare) uses two pointers moving at different speeds:
- Slow pointer moves 1 step
- Fast pointer moves 2 steps

If there's a cycle, they'll meet. Also useful for finding middle of linked list in one pass.",
                whenToUse: @"Use Fast/Slow Pointers when:
- Detecting cycles in linked lists
- Finding middle of linked list
- Finding start of cycle
- Happy number problem
- Detecting loops in sequences",
                whyItWorks: @"Fast/Slow Pointers works because:
1. In a cycle, fast catches up to slow by 1 step each iteration
2. They meet within the cycle's length iterations
3. Mathematical property: meeting point relates to cycle start",
                commonUseCases: ToJson(new[] { "Linked List Cycle", "Linked List Cycle II", "Find Middle of List", "Happy Number", "Find Duplicate Number", "Palindrome Linked List" }),
                timeComplexity: "O(n)",
                spaceComplexity: "O(1)",
                pseudoCode: @"# Detect cycle
def has_cycle(head):
    slow = fast = head
    while fast and fast.next:
        slow = slow.next
        fast = fast.next.next
        if slow == fast:
            return True
    return False

# Find cycle start
def detect_cycle(head):
    slow = fast = head
    while fast and fast.next:
        slow = slow.next
        fast = fast.next.next
        if slow == fast:
            # Find cycle start
            slow = head
            while slow != fast:
                slow = slow.next
                fast = fast.next
            return slow
    return None

# Find middle
def find_middle(head):
    slow = fast = head
    while fast and fast.next:
        slow = slow.next
        fast = fast.next.next
    return slow",
                triggerSignals: ToJson(new[] { "Cycle detection", "Find middle element", "Linked list loop", "Sequence repeats", "O(1) space with linked list" }),
                commonMistakes: ToJson(new[] { "Not checking fast.next before fast.next.next", "Wrong initialization", "Not handling empty list", "Confusing cycle detection with finding cycle start" }),
                resources: ToJson(new object[] {
                    new { title = "Floyd's Algorithm", url = "https://www.geeksforgeeks.org/floyds-cycle-finding-algorithm/", type = "article" },
                    new { title = "Linked List Cycle - LeetCode", url = "https://leetcode.com/problems/linked-list-cycle/", type = "problem" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Matrix Traversal Pattern
            Pattern.Create(
                name: "Matrix Traversal",
                description: "Techniques for traversing 2D grids: DFS, BFS, spiral, diagonal patterns.",
                category: PatternCategory.Matrix,
                whatItIs: @"Matrix Traversal covers techniques for navigating 2D arrays:
- 4-directional: up, down, left, right
- 8-directional: including diagonals
- Spiral order: outer to inner
- Diagonal traversal

Key: direction arrays simplify code.",
                whenToUse: @"Use Matrix Traversal when:
- Grid-based pathfinding
- Image processing (flood fill)
- Game boards
- Finding connected regions
- Spiral/diagonal output",
                whyItWorks: @"Matrix Traversal works because:
1. Direction arrays standardize movement
2. BFS finds shortest path in unweighted grids
3. DFS explores all connected cells
4. Boundary checks prevent index errors",
                commonUseCases: ToJson(new[] { "Number of Islands", "Flood Fill", "Rotting Oranges", "Word Search", "Spiral Matrix", "Diagonal Traverse", "Shortest Path in Grid" }),
                timeComplexity: "O(m × n) to visit all cells",
                spaceComplexity: "O(m × n) for visited array or O(min(m,n)) for BFS queue",
                pseudoCode: @"# Direction arrays
directions_4 = [(0, 1), (0, -1), (1, 0), (-1, 0)]
directions_8 = [(-1,-1), (-1,0), (-1,1), (0,-1), (0,1), (1,-1), (1,0), (1,1)]

# BFS on grid
def bfs_grid(grid, start):
    rows, cols = len(grid), len(grid[0])
    queue = deque([start])
    visited = {start}

    while queue:
        r, c = queue.popleft()
        for dr, dc in directions_4:
            nr, nc = r + dr, c + dc
            if 0 <= nr < rows and 0 <= nc < cols:
                if (nr, nc) not in visited and grid[nr][nc] == valid:
                    visited.add((nr, nc))
                    queue.append((nr, nc))

# Spiral traversal
def spiral_order(matrix):
    result = []
    top, bottom = 0, len(matrix) - 1
    left, right = 0, len(matrix[0]) - 1

    while top <= bottom and left <= right:
        # Traverse right, down, left, up
        # Adjust boundaries after each direction
    return result",
                triggerSignals: ToJson(new[] { "2D grid/matrix", "Find connected regions", "Shortest path in grid", "Flood fill", "Spiral/diagonal pattern", "Island problems" }),
                commonMistakes: ToJson(new[] { "Index out of bounds", "Not marking visited before enqueueing", "Wrong boundary conditions", "Modifying grid while iterating" }),
                resources: ToJson(new object[] {
                    new { title = "Matrix Traversal Patterns", url = "https://leetcode.com/discuss/general-discussion/657507/", type = "article" },
                    new { title = "Number of Islands", url = "https://leetcode.com/problems/number-of-islands/", type = "problem" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Top K Elements Pattern
            Pattern.Create(
                name: "Top K Elements",
                description: "Find k largest, smallest, or most frequent elements using heaps or quickselect.",
                category: PatternCategory.Heap,
                whatItIs: @"Top K Elements finds the k largest, smallest, or most frequent elements. Main approaches:
1. Heap of size k: O(n log k)
2. QuickSelect: O(n) average
3. Bucket sort for frequency: O(n)

Use min-heap for k largest, max-heap for k smallest.",
                whenToUse: @"Use Top K when:
- Finding k largest/smallest elements
- Finding k most/least frequent
- K closest points
- Need partial sorting only",
                whyItWorks: @"Top K works because:
1. Heap maintains only k elements, reducing log factor
2. QuickSelect partitions around kth element
3. Don't need full sort - only position relative to k matters",
                commonUseCases: ToJson(new[] { "Kth Largest Element", "Top K Frequent Elements", "K Closest Points", "Find K Pairs with Smallest Sums", "Kth Largest in Stream" }),
                timeComplexity: "O(n log k) with heap, O(n) with quickselect",
                spaceComplexity: "O(k)",
                pseudoCode: @"import heapq

# K largest using min-heap of size k
def k_largest(nums, k):
    heap = []
    for num in nums:
        heapq.heappush(heap, num)
        if len(heap) > k:
            heapq.heappop(heap)
    return heap

# Top K frequent
def top_k_frequent(nums, k):
    count = Counter(nums)
    return heapq.nlargest(k, count.keys(), key=count.get)

# QuickSelect for kth largest
def quick_select(nums, k):
    k = len(nums) - k  # Convert to index
    def select(l, r):
        pivot = nums[r]
        p = l
        for i in range(l, r):
            if nums[i] <= pivot:
                nums[p], nums[i] = nums[i], nums[p]
                p += 1
        nums[p], nums[r] = nums[r], nums[p]
        if p == k:
            return nums[p]
        elif p < k:
            return select(p + 1, r)
        else:
            return select(l, p - 1)
    return select(0, len(nums) - 1)",
                triggerSignals: ToJson(new[] { "K largest/smallest", "K most frequent", "K closest", "Partial sorting", "Stream of elements" }),
                commonMistakes: ToJson(new[] { "Using wrong heap type (min vs max)", "Sorting when k << n", "Not handling k > n", "QuickSelect worst case O(n²)" }),
                resources: ToJson(new object[] {
                    new { title = "Top K Elements - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/16", type = "course" },
                    new { title = "Kth Largest Element", url = "https://leetcode.com/problems/kth-largest-element-in-an-array/", type = "problem" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // K-way Merge Pattern
            Pattern.Create(
                name: "K-way Merge",
                description: "Merge K sorted arrays or lists efficiently using a min-heap.",
                category: PatternCategory.Heap,
                whatItIs: @"K-way Merge efficiently combines K sorted sequences into one sorted output. Uses a min-heap to always get the smallest current element across all lists. Each list contributes one element to the heap at a time.",
                whenToUse: @"Use K-way Merge when:
- Merging k sorted arrays/lists
- External sorting (merge sorted chunks)
- Finding smallest range covering elements from k lists
- K sorted streams",
                whyItWorks: @"K-way Merge works because:
1. Heap maintains k elements (one from each list)
2. Pop gives global minimum
3. Push replacement maintains invariant
4. Total n elements × log k heap operations",
                commonUseCases: ToJson(new[] { "Merge K Sorted Lists", "Smallest Range Covering Elements", "Kth Smallest in Sorted Matrix", "Find K Pairs with Smallest Sums", "External sort merge phase" }),
                timeComplexity: "O(n log k) where n = total elements, k = number of lists",
                spaceComplexity: "O(k) for heap",
                pseudoCode: @"import heapq

def merge_k_sorted_lists(lists):
    heap = []

    # Initialize with first element from each list
    for i, lst in enumerate(lists):
        if lst:
            heapq.heappush(heap, (lst[0].val, i, lst[0]))

    dummy = ListNode()
    current = dummy

    while heap:
        val, i, node = heapq.heappop(heap)
        current.next = node
        current = current.next

        if node.next:
            heapq.heappush(heap, (node.next.val, i, node.next))

    return dummy.next

# For arrays
def merge_k_sorted_arrays(arrays):
    heap = [(arr[0], i, 0) for i, arr in enumerate(arrays) if arr]
    heapq.heapify(heap)
    result = []

    while heap:
        val, arr_idx, elem_idx = heapq.heappop(heap)
        result.append(val)
        if elem_idx + 1 < len(arrays[arr_idx]):
            heapq.heappush(heap, (arrays[arr_idx][elem_idx + 1], arr_idx, elem_idx + 1))

    return result",
                triggerSignals: ToJson(new[] { "Merge k sorted", "Multiple sorted streams", "External sorting", "Smallest from k lists", "Global order from local orders" }),
                commonMistakes: ToJson(new[] { "Not tracking list index in heap", "Comparing nodes directly (need val)", "Not handling empty lists", "Forgetting to check for next element" }),
                resources: ToJson(new object[] {
                    new { title = "Merge K Sorted Lists", url = "https://leetcode.com/problems/merge-k-sorted-lists/", type = "problem" },
                    new { title = "K-way Merge Pattern", url = "https://www.educative.io/courses/grokking-the-coding-interview/Y5n0n3vAgYK", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Shortest Path Pattern
            Pattern.Create(
                name: "Shortest Path Algorithms",
                description: "Find shortest paths in weighted graphs: Dijkstra, Bellman-Ford, Floyd-Warshall.",
                category: PatternCategory.Graph,
                whatItIs: @"Shortest Path algorithms find minimum-cost paths in weighted graphs:
- Dijkstra: Single source, non-negative weights, O((V+E)log V)
- Bellman-Ford: Single source, handles negative weights, O(VE)
- Floyd-Warshall: All pairs, O(V³)

Choice depends on graph properties and query needs.",
                whenToUse: @"Use Dijkstra when:
- Non-negative edge weights
- Single source shortest paths
- Need efficient O((V+E)log V)

Use Bellman-Ford when:
- Negative edge weights possible
- Need to detect negative cycles

Use Floyd-Warshall when:
- Need all pairs shortest paths
- Dense graph (E ≈ V²)",
                whyItWorks: @"These algorithms work because:
1. Dijkstra: Greedy selection of minimum distance vertex is safe with non-negative weights
2. Bellman-Ford: V-1 iterations relax all edges, enough for any shortest path
3. Floyd-Warshall: Dynamic programming on intermediate vertices",
                commonUseCases: ToJson(new[] { "Network Delay Time", "Cheapest Flights Within K Stops", "Path with Minimum Effort", "Swim in Rising Water", "All pairs shortest paths" }),
                timeComplexity: "Dijkstra: O((V+E)log V), Bellman-Ford: O(VE), Floyd-Warshall: O(V³)",
                spaceComplexity: "O(V) for Dijkstra/Bellman-Ford, O(V²) for Floyd-Warshall",
                pseudoCode: @"# Dijkstra's Algorithm
def dijkstra(graph, start):
    dist = {node: float('inf') for node in graph}
    dist[start] = 0
    heap = [(0, start)]

    while heap:
        d, u = heapq.heappop(heap)
        if d > dist[u]:
            continue
        for v, weight in graph[u]:
            if dist[u] + weight < dist[v]:
                dist[v] = dist[u] + weight
                heapq.heappush(heap, (dist[v], v))

    return dist

# Bellman-Ford
def bellman_ford(edges, n, start):
    dist = [float('inf')] * n
    dist[start] = 0

    for _ in range(n - 1):
        for u, v, w in edges:
            if dist[u] + w < dist[v]:
                dist[v] = dist[u] + w

    # Check negative cycle
    for u, v, w in edges:
        if dist[u] + w < dist[v]:
            return None  # Negative cycle
    return dist",
                triggerSignals: ToJson(new[] { "Weighted graph", "Minimum cost path", "Network delay/latency", "Travel cost optimization", "Negative weights possible" }),
                commonMistakes: ToJson(new[] { "Using Dijkstra with negative weights", "Not detecting negative cycles", "Wrong priority queue usage", "Not handling disconnected nodes" }),
                resources: ToJson(new object[] {
                    new { title = "Dijkstra's Algorithm - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/17", type = "course" },
                    new { title = "Bellman-Ford Algorithm", url = "https://www.geeksforgeeks.org/bellman-ford-algorithm-dp-23/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Minimum Spanning Tree Pattern
            Pattern.Create(
                name: "Minimum Spanning Tree",
                description: "Find tree connecting all vertices with minimum total edge weight: Kruskal's and Prim's algorithms.",
                category: PatternCategory.Graph,
                whatItIs: @"A Minimum Spanning Tree (MST) connects all vertices in an undirected weighted graph with minimum total edge weight. Two classic algorithms:
- Kruskal's: Sort edges, add if no cycle (uses Union-Find)
- Prim's: Grow tree from start vertex (uses heap)

Both produce optimal MST with V-1 edges.",
                whenToUse: @"Use MST when:
- Connecting all points with minimum cost
- Network design (cables, roads)
- Clustering (remove expensive edges)
- Approximating traveling salesman",
                whyItWorks: @"MST algorithms work because:
1. Cut property: lightest edge crossing any cut is in MST
2. Kruskal's greedily picks smallest safe edge
3. Prim's greedily expands tree with cheapest edge",
                commonUseCases: ToJson(new[] { "Min Cost to Connect All Points", "Network cable/road design", "Connecting cities", "Image segmentation", "Cluster analysis" }),
                timeComplexity: "Kruskal: O(E log E), Prim: O((V+E) log V)",
                spaceComplexity: "O(V) for Union-Find or heap",
                pseudoCode: @"# Kruskal's Algorithm
def kruskal(edges, n):
    edges.sort(key=lambda x: x[2])  # Sort by weight
    uf = UnionFind(n)
    mst_weight = 0
    mst_edges = []

    for u, v, w in edges:
        if uf.union(u, v):  # No cycle
            mst_weight += w
            mst_edges.append((u, v, w))
            if len(mst_edges) == n - 1:
                break

    return mst_weight if len(mst_edges) == n - 1 else -1

# Prim's Algorithm
def prim(graph, n):
    visited = [False] * n
    heap = [(0, 0)]  # (weight, node)
    mst_weight = 0

    while heap:
        w, u = heapq.heappop(heap)
        if visited[u]:
            continue
        visited[u] = True
        mst_weight += w

        for v, weight in graph[u]:
            if not visited[v]:
                heapq.heappush(heap, (weight, v))

    return mst_weight",
                triggerSignals: ToJson(new[] { "Connect all points minimum cost", "Network design", "Spanning tree", "Minimum total weight to connect" }),
                commonMistakes: ToJson(new[] { "Using on directed graphs", "Not checking if all vertices connected", "Wrong Union-Find usage", "Duplicate edges in heap" }),
                resources: ToJson(new object[] {
                    new { title = "MST - Kruskal's", url = "https://www.geeksforgeeks.org/kruskals-minimum-spanning-tree-algorithm-greedy-algo-2/", type = "article" },
                    new { title = "MST - Prim's", url = "https://www.geeksforgeeks.org/prims-minimum-spanning-tree-mst-greedy-algo-5/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // String Matching Pattern
            Pattern.Create(
                name: "String Matching Algorithms",
                description: "Efficient algorithms for finding patterns in text: KMP, Rabin-Karp, Z-algorithm.",
                category: PatternCategory.String,
                whatItIs: @"String matching algorithms find occurrences of a pattern in text:
- KMP (Knuth-Morris-Pratt): Uses failure function to avoid re-comparing, O(n+m)
- Rabin-Karp: Rolling hash for average O(n+m), good for multiple patterns
- Z-algorithm: Computes Z-array for pattern matching, O(n+m)

All achieve linear time vs naive O(nm).",
                whenToUse: @"Use KMP when:
- Single pattern, need guaranteed O(n+m)
- Pattern has repeating prefixes

Use Rabin-Karp when:
- Multiple patterns to search
- Substring matching problems

Use Z-algorithm when:
- Pattern matching or period finding",
                whyItWorks: @"These algorithms work because:
1. KMP: Failure function tells how far to shift without missing matches
2. Rabin-Karp: Rolling hash updates in O(1), only verify hash matches
3. Z-array: Precomputed prefix lengths enable O(1) matching decisions",
                commonUseCases: ToJson(new[] { "Find substring", "Pattern matching", "Repeated substrings", "String period", "Anagram search" }),
                timeComplexity: "O(n + m) where n = text length, m = pattern length",
                spaceComplexity: "O(m) for failure/Z array",
                pseudoCode: @"# KMP Algorithm
def kmp_search(text, pattern):
    # Build failure function
    def build_lps(pattern):
        lps = [0] * len(pattern)
        length = 0
        i = 1
        while i < len(pattern):
            if pattern[i] == pattern[length]:
                length += 1
                lps[i] = length
                i += 1
            elif length > 0:
                length = lps[length - 1]
            else:
                i += 1
        return lps

    lps = build_lps(pattern)
    i = j = 0

    while i < len(text):
        if text[i] == pattern[j]:
            i += 1
            j += 1
            if j == len(pattern):
                return i - j  # Found
        elif j > 0:
            j = lps[j - 1]
        else:
            i += 1

    return -1",
                triggerSignals: ToJson(new[] { "Find pattern in text", "Multiple pattern search", "String period/repetition", "Efficient substring search" }),
                commonMistakes: ToJson(new[] { "Wrong LPS/failure function", "Hash collision handling in Rabin-Karp", "Off-by-one in indices", "Not handling empty pattern" }),
                resources: ToJson(new object[] {
                    new { title = "KMP Algorithm", url = "https://www.geeksforgeeks.org/kmp-algorithm-for-pattern-searching/", type = "article" },
                    new { title = "Rabin-Karp", url = "https://www.geeksforgeeks.org/rabin-karp-algorithm-for-pattern-searching/", type = "article" }
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
            ),

            // Segment Tree
            DataStructure.Create(
                name: "Segment Tree",
                description: "Tree structure for efficient range queries and updates on arrays.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"A Segment Tree is a binary tree where each node represents an interval of the array. The root represents the entire array, and each leaf represents a single element. Internal nodes store aggregate information (sum, min, max) of their children's intervals. Enables O(log n) range queries and updates.",
                operations: ToJson(new object[] {
                    new { name = "Build", timeComplexity = "O(n)", description = "Construct tree from array" },
                    new { name = "Range query", timeComplexity = "O(log n)", description = "Query sum/min/max over range" },
                    new { name = "Point update", timeComplexity = "O(log n)", description = "Update single element" },
                    new { name = "Range update", timeComplexity = "O(log n)", description = "With lazy propagation" }
                }),
                whenToUse: @"Use Segment Tree when:
- Need range queries (sum, min, max, GCD)
- Need point or range updates
- Multiple queries on same array
- Competitive programming",
                tradeoffs: @"Pros:
- O(log n) range queries and updates
- Flexible aggregate functions
- Supports lazy propagation

Cons:
- O(n) space (typically 4n)
- Complex implementation
- Overkill for simple problems",
                commonUseCases: ToJson(new[] { "Range Sum Query - Mutable", "Range Minimum Query", "Count of Smaller Numbers After Self", "Falling Squares", "Rectangle Area II" }),
                implementation: @"class SegmentTree:
    def __init__(self, nums):
        self.n = len(nums)
        self.tree = [0] * (4 * self.n)
        self._build(nums, 0, 0, self.n - 1)

    def _build(self, nums, node, start, end):
        if start == end:
            self.tree[node] = nums[start]
        else:
            mid = (start + end) // 2
            self._build(nums, 2*node+1, start, mid)
            self._build(nums, 2*node+2, mid+1, end)
            self.tree[node] = self.tree[2*node+1] + self.tree[2*node+2]

    def update(self, idx, val):
        self._update(0, 0, self.n - 1, idx, val)

    def _update(self, node, start, end, idx, val):
        if start == end:
            self.tree[node] = val
        else:
            mid = (start + end) // 2
            if idx <= mid:
                self._update(2*node+1, start, mid, idx, val)
            else:
                self._update(2*node+2, mid+1, end, idx, val)
            self.tree[node] = self.tree[2*node+1] + self.tree[2*node+2]

    def query(self, left, right):
        return self._query(0, 0, self.n - 1, left, right)

    def _query(self, node, start, end, left, right):
        if right < start or left > end:
            return 0
        if left <= start and end <= right:
            return self.tree[node]
        mid = (start + end) // 2
        return self._query(2*node+1, start, mid, left, right) + \
               self._query(2*node+2, mid+1, end, left, right)",
                resources: ToJson(new object[] {
                    new { title = "Segment Tree - CP Algorithms", url = "https://cp-algorithms.com/data_structures/segment_tree.html", type = "article" },
                    new { title = "Segment Tree Tutorial", url = "https://www.geeksforgeeks.org/segment-tree-data-structure/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Fenwick Tree (Binary Indexed Tree)
            DataStructure.Create(
                name: "Fenwick Tree (Binary Indexed Tree)",
                description: "Space-efficient structure for prefix sums with O(log n) updates and queries.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"A Fenwick Tree (Binary Indexed Tree or BIT) efficiently computes prefix sums and supports point updates. Uses clever bit manipulation to determine parent-child relationships. More space-efficient than Segment Tree (uses n elements vs 4n).",
                operations: ToJson(new object[] {
                    new { name = "Build", timeComplexity = "O(n log n)", description = "Initialize from array" },
                    new { name = "Point update", timeComplexity = "O(log n)", description = "Add value at index" },
                    new { name = "Prefix sum", timeComplexity = "O(log n)", description = "Sum from 0 to index" },
                    new { name = "Range sum", timeComplexity = "O(log n)", description = "Sum from i to j" }
                }),
                whenToUse: @"Use Fenwick Tree when:
- Need prefix sums with updates
- Counting inversions
- Range sum queries
- Simpler than Segment Tree suffices",
                tradeoffs: @"Pros:
- O(n) space (more efficient than Segment Tree)
- Simple implementation
- Fast in practice

Cons:
- Less flexible than Segment Tree
- Only works for associative, invertible operations
- Cannot do range updates easily",
                commonUseCases: ToJson(new[] { "Range Sum Query - Mutable", "Count of Smaller Numbers After Self", "Count inversions", "2D range sums" }),
                implementation: @"class FenwickTree:
    def __init__(self, n):
        self.n = n
        self.tree = [0] * (n + 1)

    def update(self, i, delta):
        '''Add delta to index i (1-indexed)'''
        while i <= self.n:
            self.tree[i] += delta
            i += i & (-i)  # Add lowest set bit

    def prefix_sum(self, i):
        '''Sum from index 1 to i'''
        total = 0
        while i > 0:
            total += self.tree[i]
            i -= i & (-i)  # Remove lowest set bit
        return total

    def range_sum(self, left, right):
        '''Sum from left to right (1-indexed)'''
        return self.prefix_sum(right) - self.prefix_sum(left - 1)

# Build from array
def build_fenwick(nums):
    bit = FenwickTree(len(nums))
    for i, num in enumerate(nums):
        bit.update(i + 1, num)
    return bit",
                resources: ToJson(new object[] {
                    new { title = "Fenwick Tree - CP Algorithms", url = "https://cp-algorithms.com/data_structures/fenwick.html", type = "article" },
                    new { title = "BIT Tutorial", url = "https://www.topcoder.com/thrive/articles/Binary%20Indexed%20Trees", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // AVL Tree
            DataStructure.Create(
                name: "AVL Tree",
                description: "Self-balancing BST maintaining height balance for guaranteed O(log n) operations.",
                category: DataStructureCategory.Tree,
                whatItIs: @"An AVL Tree is a self-balancing Binary Search Tree where the heights of left and right subtrees differ by at most 1 for every node. Named after Adelson-Velsky and Landis. Uses rotations to maintain balance after insertions and deletions.",
                operations: ToJson(new object[] {
                    new { name = "Search", timeComplexity = "O(log n)", description = "Guaranteed balanced height" },
                    new { name = "Insert", timeComplexity = "O(log n)", description = "Insert + rebalance" },
                    new { name = "Delete", timeComplexity = "O(log n)", description = "Delete + rebalance" },
                    new { name = "Min/Max", timeComplexity = "O(log n)", description = "Leftmost/rightmost" }
                }),
                whenToUse: @"Use AVL Tree when:
- Need guaranteed O(log n) operations
- Frequent searches (more balanced than Red-Black)
- Ordered data with strict time bounds",
                tradeoffs: @"Pros:
- Strictly balanced (height ≤ 1.44 log n)
- Faster lookups than Red-Black Tree
- Guaranteed O(log n) worst case

Cons:
- More rotations on insert/delete
- Slower insertions than Red-Black
- Extra height storage per node",
                commonUseCases: ToJson(new[] { "Database indexing", "In-memory sorted data", "Interval trees", "Priority scheduling" }),
                implementation: @"class AVLNode:
    def __init__(self, val):
        self.val = val
        self.left = self.right = None
        self.height = 1

def get_height(node):
    return node.height if node else 0

def get_balance(node):
    return get_height(node.left) - get_height(node.right) if node else 0

def right_rotate(y):
    x = y.left
    T2 = x.right
    x.right = y
    y.left = T2
    y.height = 1 + max(get_height(y.left), get_height(y.right))
    x.height = 1 + max(get_height(x.left), get_height(x.right))
    return x

def left_rotate(x):
    y = x.right
    T2 = y.left
    y.left = x
    x.right = T2
    x.height = 1 + max(get_height(x.left), get_height(x.right))
    y.height = 1 + max(get_height(y.left), get_height(y.right))
    return y

def insert(root, val):
    if not root:
        return AVLNode(val)
    if val < root.val:
        root.left = insert(root.left, val)
    else:
        root.right = insert(root.right, val)

    root.height = 1 + max(get_height(root.left), get_height(root.right))
    balance = get_balance(root)

    # Left Left
    if balance > 1 and val < root.left.val:
        return right_rotate(root)
    # Right Right
    if balance < -1 and val > root.right.val:
        return left_rotate(root)
    # Left Right
    if balance > 1 and val > root.left.val:
        root.left = left_rotate(root.left)
        return right_rotate(root)
    # Right Left
    if balance < -1 and val < root.right.val:
        root.right = right_rotate(root.right)
        return left_rotate(root)

    return root",
                resources: ToJson(new object[] {
                    new { title = "AVL Tree - GeeksforGeeks", url = "https://www.geeksforgeeks.org/introduction-to-avl-tree/", type = "article" },
                    new { title = "AVL Tree Visualization", url = "https://www.cs.usfca.edu/~galles/visualization/AVLtree.html", type = "visualization" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // LRU Cache
            DataStructure.Create(
                name: "LRU Cache",
                description: "Cache with Least Recently Used eviction policy using hash map and doubly linked list.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"An LRU (Least Recently Used) Cache is a data structure that stores a limited number of items and evicts the least recently accessed item when full. Combines a hash map for O(1) lookup with a doubly linked list for O(1) recency tracking.",
                operations: ToJson(new object[] {
                    new { name = "Get", timeComplexity = "O(1)", description = "Retrieve value and mark as recently used" },
                    new { name = "Put", timeComplexity = "O(1)", description = "Insert/update and evict if full" },
                    new { name = "Delete", timeComplexity = "O(1)", description = "Remove specific key" }
                }),
                whenToUse: @"Use LRU Cache when:
- Need caching with size limit
- Recently used items are more valuable
- Implementing memoization
- Database/web caching",
                tradeoffs: @"Pros:
- O(1) all operations
- Automatic eviction
- Good cache hit rates for temporal locality

Cons:
- Extra memory for linked list
- Not optimal for all access patterns
- More complex than simple cache",
                commonUseCases: ToJson(new[] { "LRU Cache problem", "Web browser cache", "Database buffer pool", "CPU cache simulation", "API response caching" }),
                implementation: @"from collections import OrderedDict

class LRUCache:
    def __init__(self, capacity: int):
        self.cache = OrderedDict()
        self.capacity = capacity

    def get(self, key: int) -> int:
        if key not in self.cache:
            return -1
        self.cache.move_to_end(key)  # Mark as recently used
        return self.cache[key]

    def put(self, key: int, value: int) -> None:
        if key in self.cache:
            self.cache.move_to_end(key)
        self.cache[key] = value
        if len(self.cache) > self.capacity:
            self.cache.popitem(last=False)  # Remove oldest

# Manual implementation with doubly linked list
class DLinkedNode:
    def __init__(self, key=0, val=0):
        self.key = key
        self.val = val
        self.prev = None
        self.next = None

class LRUCacheManual:
    def __init__(self, capacity):
        self.cache = {}
        self.capacity = capacity
        self.head = DLinkedNode()  # Dummy head
        self.tail = DLinkedNode()  # Dummy tail
        self.head.next = self.tail
        self.tail.prev = self.head

    def _remove(self, node):
        node.prev.next = node.next
        node.next.prev = node.prev

    def _add_to_head(self, node):
        node.next = self.head.next
        node.prev = self.head
        self.head.next.prev = node
        self.head.next = node",
                resources: ToJson(new object[] {
                    new { title = "LRU Cache - LeetCode", url = "https://leetcode.com/problems/lru-cache/", type = "problem" },
                    new { title = "LRU Cache Design", url = "https://www.geeksforgeeks.org/lru-cache-implementation/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Skip List
            DataStructure.Create(
                name: "Skip List",
                description: "Probabilistic data structure with O(log n) search using multiple linked list layers.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"A Skip List is a probabilistic data structure that uses multiple layers of linked lists to achieve O(log n) average search time. Each element has a random height determining which layers it appears in. Higher layers act as 'express lanes' for faster traversal.",
                operations: ToJson(new object[] {
                    new { name = "Search", timeComplexity = "O(log n)*", description = "Average case, O(n) worst" },
                    new { name = "Insert", timeComplexity = "O(log n)*", description = "Average case" },
                    new { name = "Delete", timeComplexity = "O(log n)*", description = "Average case" },
                    new { name = "Range query", timeComplexity = "O(log n + k)", description = "k = number of results" }
                }),
                whenToUse: @"Use Skip List when:
- Need ordered data with fast operations
- Simpler alternative to balanced trees
- Concurrent access needed (lock-free variants)
- Range queries required",
                tradeoffs: @"Pros:
- Simple to implement vs balanced trees
- Good for concurrent access
- No rotations needed
- Efficient range queries

Cons:
- Probabilistic guarantees (not worst-case)
- More space than BST
- Cache-unfriendly",
                commonUseCases: ToJson(new[] { "Redis sorted sets", "LevelDB/RocksDB", "Concurrent data structures", "In-memory indexes" }),
                implementation: @"import random

class SkipListNode:
    def __init__(self, val, level):
        self.val = val
        self.forward = [None] * (level + 1)

class SkipList:
    def __init__(self, max_level=16, p=0.5):
        self.max_level = max_level
        self.p = p
        self.level = 0
        self.head = SkipListNode(float('-inf'), max_level)

    def random_level(self):
        level = 0
        while random.random() < self.p and level < self.max_level:
            level += 1
        return level

    def search(self, target):
        current = self.head
        for i in range(self.level, -1, -1):
            while current.forward[i] and current.forward[i].val < target:
                current = current.forward[i]
        current = current.forward[0]
        return current and current.val == target

    def insert(self, val):
        update = [None] * (self.max_level + 1)
        current = self.head

        for i in range(self.level, -1, -1):
            while current.forward[i] and current.forward[i].val < val:
                current = current.forward[i]
            update[i] = current

        level = self.random_level()
        if level > self.level:
            for i in range(self.level + 1, level + 1):
                update[i] = self.head
            self.level = level

        new_node = SkipListNode(val, level)
        for i in range(level + 1):
            new_node.forward[i] = update[i].forward[i]
            update[i].forward[i] = new_node",
                resources: ToJson(new object[] {
                    new { title = "Skip List - Wikipedia", url = "https://en.wikipedia.org/wiki/Skip_list", type = "article" },
                    new { title = "Skip List Tutorial", url = "https://www.geeksforgeeks.org/skip-list/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Monotonic Queue
            DataStructure.Create(
                name: "Monotonic Queue",
                description: "Queue maintaining elements in sorted order for efficient sliding window min/max.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A Monotonic Queue (or Monotonic Deque) maintains elements in increasing or decreasing order. When adding an element, removes all elements that violate the monotonic property. Combined with index tracking, enables O(1) amortized sliding window min/max queries.",
                operations: ToJson(new object[] {
                    new { name = "Push", timeComplexity = "O(1)*", description = "Amortized, may remove multiple elements" },
                    new { name = "Pop front", timeComplexity = "O(1)", description = "Remove expired elements" },
                    new { name = "Get min/max", timeComplexity = "O(1)", description = "Front of queue" }
                }),
                whenToUse: @"Use Monotonic Queue when:
- Sliding window minimum/maximum
- Need to maintain sorted window
- Next greater/smaller with window",
                tradeoffs: @"Pros:
- O(1) amortized operations
- Perfect for sliding window problems
- Each element pushed/popped once

Cons:
- Specific use case
- More complex than simple queue
- Only maintains one extreme",
                commonUseCases: ToJson(new[] { "Sliding Window Maximum", "Shortest Subarray with Sum at Least K", "Jump Game VI", "Constrained Subsequence Sum" }),
                implementation: @"from collections import deque

class MonotonicQueue:
    '''Maintains decreasing order for max queries'''
    def __init__(self):
        self.dq = deque()  # Stores (value, index)

    def push(self, val, idx):
        # Remove smaller elements (they'll never be max)
        while self.dq and self.dq[-1][0] <= val:
            self.dq.pop()
        self.dq.append((val, idx))

    def pop_expired(self, left_bound):
        # Remove elements outside window
        while self.dq and self.dq[0][1] < left_bound:
            self.dq.popleft()

    def get_max(self):
        return self.dq[0][0] if self.dq else None

# Sliding window maximum
def maxSlidingWindow(nums, k):
    mq = MonotonicQueue()
    result = []

    for i, num in enumerate(nums):
        mq.push(num, i)
        mq.pop_expired(i - k + 1)
        if i >= k - 1:
            result.append(mq.get_max())

    return result",
                resources: ToJson(new object[] {
                    new { title = "Sliding Window Maximum", url = "https://leetcode.com/problems/sliding-window-maximum/", type = "problem" },
                    new { title = "Monotonic Queue Explained", url = "https://www.geeksforgeeks.org/sliding-window-maximum-maximum-of-all-subarrays-of-size-k/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Graph (Adjacency Matrix)
            DataStructure.Create(
                name: "Graph (Adjacency Matrix)",
                description: "Graph represented as 2D matrix where matrix[i][j] indicates edge between vertices i and j.",
                category: DataStructureCategory.Graph,
                whatItIs: @"An Adjacency Matrix represents a graph as a 2D array where entry [i][j] indicates if there's an edge from vertex i to j (and its weight for weighted graphs). O(1) edge lookup but O(V²) space regardless of edge count.",
                operations: ToJson(new object[] {
                    new { name = "Check edge", timeComplexity = "O(1)", description = "Direct array access" },
                    new { name = "Add edge", timeComplexity = "O(1)", description = "Set matrix entry" },
                    new { name = "Remove edge", timeComplexity = "O(1)", description = "Clear matrix entry" },
                    new { name = "Get all neighbors", timeComplexity = "O(V)", description = "Scan entire row" },
                    new { name = "Space", timeComplexity = "O(V²)", description = "Fixed regardless of edges" }
                }),
                whenToUse: @"Use Adjacency Matrix when:
- Graph is dense (E ≈ V²)
- Need O(1) edge existence checks
- Working with weighted graphs
- Floyd-Warshall or similar algorithms",
                tradeoffs: @"Pros:
- O(1) edge lookup
- Simple implementation
- Good for dense graphs
- Easy to represent weighted edges

Cons:
- O(V²) space always
- Wasteful for sparse graphs
- O(V) to find all neighbors",
                commonUseCases: ToJson(new[] { "Floyd-Warshall", "Dense graph algorithms", "Small graphs", "Graph with many edge queries" }),
                implementation: @"# Create adjacency matrix
def create_adj_matrix(n, edges, directed=False):
    matrix = [[0] * n for _ in range(n)]
    for u, v in edges:
        matrix[u][v] = 1
        if not directed:
            matrix[v][u] = 1
    return matrix

# Weighted graph
def create_weighted_matrix(n, edges, directed=False):
    INF = float('inf')
    matrix = [[INF] * n for _ in range(n)]
    for i in range(n):
        matrix[i][i] = 0
    for u, v, w in edges:
        matrix[u][v] = w
        if not directed:
            matrix[v][u] = w
    return matrix

# Check edge
def has_edge(matrix, u, v):
    return matrix[u][v] != 0  # or != INF for weighted

# Get neighbors
def get_neighbors(matrix, u):
    return [v for v in range(len(matrix)) if matrix[u][v] != 0]",
                resources: ToJson(new object[] {
                    new { title = "Graph Representations", url = "https://www.geeksforgeeks.org/graph-and-its-representations/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Circular Buffer
            DataStructure.Create(
                name: "Circular Buffer (Ring Buffer)",
                description: "Fixed-size buffer that wraps around, useful for streaming data and producer-consumer patterns.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A Circular Buffer (Ring Buffer) is a fixed-size queue that wraps around when reaching the end. Uses two pointers (head and tail) with modulo arithmetic. When full, new data overwrites the oldest data. Efficient for streaming and bounded queues.",
                operations: ToJson(new object[] {
                    new { name = "Enqueue", timeComplexity = "O(1)", description = "Add to tail" },
                    new { name = "Dequeue", timeComplexity = "O(1)", description = "Remove from head" },
                    new { name = "Peek", timeComplexity = "O(1)", description = "View head element" },
                    new { name = "IsFull/IsEmpty", timeComplexity = "O(1)", description = "Check buffer state" }
                }),
                whenToUse: @"Use Circular Buffer when:
- Fixed memory budget
- Streaming/real-time data
- Producer-consumer patterns
- Audio/video buffering
- Logging recent events",
                tradeoffs: @"Pros:
- Fixed memory usage
- O(1) all operations
- No memory allocation after init
- Cache-friendly

Cons:
- Fixed capacity
- Loses old data when full
- Slightly complex pointer management",
                commonUseCases: ToJson(new[] { "Design Circular Queue", "Moving average from data stream", "Audio buffers", "Network packet buffers", "Log rotation" }),
                implementation: @"class CircularBuffer:
    def __init__(self, capacity):
        self.buffer = [None] * capacity
        self.capacity = capacity
        self.head = 0  # Read position
        self.tail = 0  # Write position
        self.size = 0

    def is_empty(self):
        return self.size == 0

    def is_full(self):
        return self.size == self.capacity

    def enqueue(self, val):
        if self.is_full():
            return False  # Or overwrite: self.head = (self.head + 1) % self.capacity
        self.buffer[self.tail] = val
        self.tail = (self.tail + 1) % self.capacity
        self.size += 1
        return True

    def dequeue(self):
        if self.is_empty():
            return None
        val = self.buffer[self.head]
        self.head = (self.head + 1) % self.capacity
        self.size -= 1
        return val

    def front(self):
        return self.buffer[self.head] if not self.is_empty() else None

    def rear(self):
        if self.is_empty():
            return None
        return self.buffer[(self.tail - 1) % self.capacity]",
                resources: ToJson(new object[] {
                    new { title = "Design Circular Queue - LeetCode", url = "https://leetcode.com/problems/design-circular-queue/", type = "problem" },
                    new { title = "Circular Buffer - Wikipedia", url = "https://en.wikipedia.org/wiki/Circular_buffer", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            )
        ];
    }
}
