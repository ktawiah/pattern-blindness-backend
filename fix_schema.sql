-- Create missing tables for PatternBlindness database

-- LeetCodeProblemCache table
CREATE TABLE IF NOT EXISTS "LeetCodeProblemCache" (
    "Id" uuid NOT NULL,
    "Slug" character varying(200) NOT NULL,
    "Title" character varying(500) NOT NULL,
    "TitleSlug" character varying(200) NOT NULL,
    "Content" text NOT NULL,
    "Difficulty" character varying(20) NOT NULL,
    "Tags" jsonb NOT NULL DEFAULT '[]',
    "Examples" jsonb NOT NULL DEFAULT '[]',
    "Constraints" jsonb NOT NULL DEFAULT '[]',
    "SimilarQuestions" jsonb NOT NULL DEFAULT '[]',
    "CategoryTitle" character varying(100),
    "IsPaidOnly" boolean NOT NULL DEFAULT false,
    "CachedAt" timestamp with time zone NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_LeetCodeProblemCache" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_LeetCodeProblemCache_Slug" ON "LeetCodeProblemCache" ("Slug");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_LeetCodeProblemCache_TitleSlug" ON "LeetCodeProblemCache" ("TitleSlug");

-- ProblemAnalyses table
CREATE TABLE IF NOT EXISTS "ProblemAnalyses" (
    "Id" uuid NOT NULL,
    "LeetCodeProblemCacheId" uuid NOT NULL,
    "PrimaryPatterns" jsonb NOT NULL DEFAULT '[]',
    "SecondaryPatterns" jsonb NOT NULL DEFAULT '[]',
    "AntiPatterns" jsonb NOT NULL DEFAULT '[]',
    "KeySignals" jsonb NOT NULL DEFAULT '[]',
    "Explanation" text NOT NULL DEFAULT '',
    "ModelUsed" character varying(50) NOT NULL DEFAULT 'gpt-4o',
    "RawResponse" text,
    "AnalyzedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_ProblemAnalyses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ProblemAnalyses_LeetCodeProblemCache" FOREIGN KEY ("LeetCodeProblemCacheId") 
        REFERENCES "LeetCodeProblemCache" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProblemAnalyses_LeetCodeProblemCacheId" ON "ProblemAnalyses" ("LeetCodeProblemCacheId");

-- Reflections table
CREATE TABLE IF NOT EXISTS "Reflections" (
    "Id" uuid NOT NULL,
    "AttemptId" uuid NOT NULL,
    "UserColdStartSummary" text,
    "WasPatternCorrect" boolean NOT NULL DEFAULT false,
    "Feedback" text NOT NULL DEFAULT '',
    "CorrectIdentifications" jsonb NOT NULL DEFAULT '[]',
    "MissedSignals" jsonb NOT NULL DEFAULT '[]',
    "NextTimeAdvice" text NOT NULL DEFAULT '',
    "PatternTips" text NOT NULL DEFAULT '',
    "ConfidenceCalibration" text NOT NULL DEFAULT '',
    "ModelUsed" character varying(50) NOT NULL DEFAULT 'gpt-4o',
    "RawResponse" text,
    "GeneratedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Reflections" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Reflections_Attempts" FOREIGN KEY ("AttemptId") 
        REFERENCES "Attempts" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Reflections_AttemptId" ON "Reflections" ("AttemptId");

-- Add missing columns to Attempts table if needed
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Attempts' AND column_name='LeetCodeProblemCacheId') THEN
        ALTER TABLE "Attempts" ADD COLUMN "LeetCodeProblemCacheId" uuid;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Attempts' AND column_name='ChosenPatternName') THEN
        ALTER TABLE "Attempts" ADD COLUMN "ChosenPatternName" character varying(100);
    END IF;
END $$;

-- Add FK constraint if not exists
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name='FK_Attempts_LeetCodeProblemCache') THEN
        ALTER TABLE "Attempts" ADD CONSTRAINT "FK_Attempts_LeetCodeProblemCache" 
            FOREIGN KEY ("LeetCodeProblemCacheId") REFERENCES "LeetCodeProblemCache" ("Id");
    END IF;
END $$;

-- Create index on LeetCodeProblemCacheId if not exists
CREATE INDEX IF NOT EXISTS "IX_Attempts_LeetCodeProblemCacheId" ON "Attempts" ("LeetCodeProblemCacheId");
