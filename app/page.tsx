"use client";

import { useState, useEffect } from "react";

// Mock data with rich details, categories, and tags
const sampleJobs = [
  { 
    id: 1, 
    title: "Frontend Developer", 
    company: "TechStart", 
    type: "Remote", 
    xpReward: 50, 
    difficulty: "Medium",
    skills: ["React", "Tailwind", "Next.js"]
  },
  { 
    id: 2, 
    title: "Full-Stack Engineer", 
    company: "CloudScale", 
    type: "Hybrid", 
    xpReward: 80, 
    difficulty: "Hard",
    skills: ["Node.js", "PostgreSQL", "React"]
  },
  { 
    id: 3, 
    title: "UI/UX Designer", 
    company: "CreativePulse", 
    type: "Contract", 
    xpReward: 40, 
    difficulty: "Easy",
    skills: ["Figma", "Design Systems", "Prototyping"]
  },
];

// Badge Milestones
const badgesList = [
  { id: "first_quest", name: "First Blood", description: "Completed your first quest", icon: "🏹", condition: (completed: number[], xp: number) => completed.length >= 1 },
  { id: "hard_mode", name: "Beast Slayer", description: "Conquered a Hard Mission", icon: "⚔️", condition: (completed: number[], xp: number) => completed.includes(2) },
  { id: "xp_hoarder", name: "XP Hoarder", description: "Reached 100+ Total XP", icon: "👑", condition: (completed: number[], xp: number) => xp >= 100 },
];

export default function Home() {
  // --- STATE MANAGEMENT ---
  const [xp, setXp] = useState(0);
  const [streak, setStreak] = useState(3);
  const [completedQuests, setCompletedQuests] = useState<number[]>([]);
  
  // Leveling Logic (100 XP per Level)
  const currentLevel = Math.floor(xp / 100) + 1;
  const xpProgress = xp % 100; // Progress bar percentage
  const [prevLevel, setPrevLevel] = useState(1); // Track level history to trigger modals
  
  // Custom Alert / Modal State
  const [showLevelUpModal, setShowLevelUpModal] = useState(false);
  const [levelUpNum, setLevelUpNum] = useState(1);

  // Monitor level changes to trigger level-up popups
  useEffect(() => {
    if (currentLevel > prevLevel) {
      setLevelUpNum(currentLevel);
      setShowLevelUpModal(true);
      setPrevLevel(currentLevel); // Update level history
    }
  }, [currentLevel, prevLevel]);

  // Handle completing a job quest and staging backend API connectivity
  const handleAcceptQuest = async (jobId: number, xpReward: number) => {
    if (completedQuests.includes(jobId)) return;

    // 1. Instantly update the local UI states so the platform feels ultra-responsive
    setXp((prevXp) => prevXp + xpReward);
    setCompletedQuests((prev) => [...prev, jobId]);

    // 2. BACKEND & UNITY INTEGRATION BRIDGE
    // This tells your backend dev and Michael exactly how data will sync to the server database
    try {
      const response = await fetch("https://api.yourproject.com/quests/complete", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          userId: "current_user_id", // Placeholder for profile identifier
          jobId: jobId,
          xpGained: xpReward
        }),
      });

      if (!response.ok) {
        console.error("Backend synchronization failed.");
      }
    } catch (error) {
      // Gentle sandbox fallback so your application continues working offline/locally perfectly!
      console.log("Central server offline - operating smoothly in local-only sandbox environment.", error);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900 font-sans pb-16">
      
      {/* --- LEVEL UP MODAL --- */}
      {showLevelUpModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm transition-opacity duration-300">
          <div className="bg-white rounded-2xl p-8 max-w-sm w-full mx-4 border border-amber-200 text-center shadow-2xl relative overflow-hidden">
            
            {/* Sparkle background elements */}
            <div className="absolute top-0 left-0 w-full h-2 bg-gradient-to-r from-amber-400 via-pink-500 to-purple-600"></div>
            
            {/* Trophy Icon */}
            <div className="w-20 h-20 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4 text-4xl animate-bounce">
              🎉
            </div>
            
            <h3 className="text-3xl font-extrabold text-transparent bg-clip-text bg-gradient-to-r from-amber-500 to-amber-700">
              LEVEL UP!
            </h3>
            
            <p className="text-slate-500 mt-1 font-medium text-sm tracking-wide uppercase">New Ranking Achieved</p>
            
            <div className="my-5 bg-gradient-to-b from-amber-50 to-amber-100/50 rounded-xl py-3 border border-amber-200/50">
              <span className="text-slate-400 font-bold text-xs uppercase block">Current Tier</span>
              <span className="text-4xl font-black text-amber-600 block mt-1">LVL {levelUpNum}</span>
            </div>

            <p className="text-slate-600 text-sm leading-relaxed px-2">
              Incredible work, adventurer! You are gaining momentum and unlocking harder quests. Keep applying to level up further!
            </p>

            <button
              onClick={() => setShowLevelUpModal(false)}
              className="mt-6 w-full py-3 bg-slate-900 hover:bg-slate-800 text-white font-bold rounded-xl transition duration-200 active:scale-[0.98]"
            >
              Continue Questing
            </button>
          </div>
        </div>
      )}

      {/* --- NAVIGATION BAR --- */}
      <nav className="border-b border-slate-200 bg-white/95 backdrop-blur px-6 py-4 flex flex-col md:flex-row items-center justify-between sticky top-0 z-40 gap-4 shadow-sm">
        <div className="text-xl font-bold tracking-tight bg-slate-900 text-white px-3 py-1 rounded-lg">
          Job<span className="text-amber-400">Finder</span>
        </div>
        
        {/* Live Game HUD (Heads-Up Display) */}
        <div className="flex flex-wrap items-center gap-4 md:gap-6 text-sm font-medium w-full md:w-auto justify-end">
          
          {/* Streak Counter */}
          <span className="bg-amber-50 text-amber-800 px-3 py-1.5 rounded-lg font-bold border border-amber-200 flex items-center gap-1.5 shadow-sm">
            🔥 {streak} Day Streak
          </span>

          {/* XP & Level Dashboard Progress Bar */}
          <div className="flex flex-col min-w-[200px] bg-slate-100 p-2 rounded-xl border border-slate-200">
            <div className="flex justify-between items-center text-xs font-bold text-slate-700 px-1 mb-1">
              <span>LEVEL {currentLevel}</span>
              <span className="text-purple-700">{xpProgress}/100 XP</span>
            </div>
            <div className="w-full bg-slate-200 h-2.5 rounded-full overflow-hidden">
              <div 
                className="bg-gradient-to-r from-purple-500 to-indigo-600 h-full rounded-full transition-all duration-500 ease-out"
                style={{ width: `${xpProgress}%` }}
              ></div>
            </div>
          </div>

          <div className="flex items-center gap-4 text-slate-500">
            <a href="#" className="hover:text-slate-900 font-bold text-slate-900">Quests</a>
            <a href="#" className="hover:text-slate-900 transition font-bold">Dashboard</a>
          </div>
        </div>
      </nav>

      {/* --- MAIN PAGE CONTENT --- */}
      <main className="max-w-7xl mx-auto px-6 py-10 grid grid-cols-1 lg:grid-cols-4 gap-8">
        
        {/* LEFT COLUMN: ACTIVE QUESTS */}
        <section className="lg:col-span-3">
          <div className="flex items-center justify-between mb-6">
            <div>
              <h2 className="text-2xl font-black tracking-tight text-slate-900">Available Quests</h2>
              <p className="text-slate-500 text-sm">Accept and complete applications to gain experience points.</p>
            </div>
            
            {/* Quick stats counter */}
            <span className="text-xs bg-slate-200/60 font-bold text-slate-700 px-3 py-1.5 rounded-full">
              Completed: {completedQuests.length} / {sampleJobs.length}
            </span>
          </div>
          
          {/* Job Card Grid */}
          <div className="grid gap-6 md:grid-cols-2">
            {sampleJobs.map((job) => {
              const isCompleted = completedQuests.includes(job.id);
              
              return (
                <div 
                  key={job.id} 
                  className={`border rounded-2xl p-6 bg-white shadow-sm hover:shadow-lg hover:-translate-y-1 transition-all duration-300 flex flex-col justify-between ${
                    isCompleted 
                      ? "border-emerald-300 bg-emerald-50/20 opacity-80 shadow-none hover:translate-y-0 hover:shadow-none" 
                      : "border-slate-200"
                  }`}
                >
                  <div>
                    {/* Card Header & Difficulty Tag */}
                    <div className="flex justify-between items-start mb-4">
                      <span className={`text-xs font-black uppercase tracking-wider px-2.5 py-1 rounded-md ${
                        job.difficulty === "Hard" ? "bg-rose-50 text-rose-700 border border-rose-100" :
                        job.difficulty === "Medium" ? "bg-sky-50 text-sky-700 border border-sky-100" : 
                        "bg-emerald-50 text-emerald-700 border border-emerald-100"
                      }`}>
                        {job.difficulty} Quest
                      </span>
                      
                      {isCompleted && (
                        <span className="text-xs font-bold text-emerald-700 bg-emerald-100 px-2 py-0.5 rounded flex items-center gap-1">
                          ✓ Claimed
                        </span>
                      )}
                    </div>

                    <h3 className="text-xl font-bold text-slate-900">{job.title}</h3>
                    <p className="text-sm text-slate-500 mt-1 font-semibold">{job.company} • {job.type}</p>

                    {/* Skill tags */}
                    <div className="flex flex-wrap gap-1.5 mt-3 mb-5">
                      {job.skills.map((skill, idx) => (
                        <span key={idx} className="bg-slate-100 text-slate-600 text-xs px-2 py-0.5 rounded font-medium">
                          {skill}
                        </span>
                      ))}
                    </div>
                  </div>

                  <div>
                    {/* Reward Box */}
                    <div className="bg-purple-50 rounded-xl p-3 flex justify-between items-center mb-4 border border-purple-100/60">
                      <span className="text-xs font-bold text-purple-700 uppercase tracking-wider">Completion Reward:</span>
                      <span className="text-sm font-black text-purple-950">+{job.xpReward} XP</span>
                    </div>

                    {/* Quest Button */}
                    <button
                      onClick={() => handleAcceptQuest(job.id, job.xpReward)}
                      disabled={isCompleted}
                      className={`w-full py-3 px-4 rounded-xl font-bold text-sm transition-all duration-200 ${
                        isCompleted 
                          ? "bg-emerald-600 text-white cursor-not-allowed" 
                          : "bg-slate-900 text-white hover:bg-slate-800 hover:scale-[1.01] active:scale-[0.98]"
                      }`}
                    >
                      {isCompleted ? "✓ Quest Completed" : "Accept Quest & Apply"}
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        </section>

        {/* RIGHT COLUMN: ACHIEVEMENTS PANEL */}
        <aside className="lg:col-span-1">
          <div className="bg-white rounded-2xl p-6 border border-slate-200 shadow-sm sticky top-28">
            <h3 className="text-lg font-black text-slate-900 mb-4 flex items-center gap-2">
              🏆 Unlocked Badges
            </h3>
            
            {/* Badges Stack */}
            <div className="space-y-4">
              {badgesList.map((badge) => {
                const isUnlocked = badge.condition(completedQuests, xp);
                
                return (
                  <div 
                    key={badge.id} 
                    className={`flex gap-3 p-3.5 rounded-xl border transition-all duration-300 ${
                      isUnlocked 
                        ? "bg-amber-50/40 border-amber-200" 
                        : "bg-slate-50 border-slate-100 opacity-50 grayscale"
                    }`}
                  >
                    {/* Badge Icon circle */}
                    <div className={`w-11 h-11 rounded-full flex items-center justify-center text-xl shrink-0 shadow-sm ${
                      isUnlocked ? "bg-amber-100" : "bg-slate-200"
                    }`}>
                      {badge.icon}
                    </div>
                    
                    {/* Badge details */}
                    <div>
                      <h4 className={`text-sm font-extrabold ${isUnlocked ? "text-amber-900" : "text-slate-600"}`}>
                        {badge.name}
                      </h4>
                      <p className="text-xs text-slate-500 mt-0.5 leading-tight">{badge.description}</p>
                      
                      {/* Interactive Unlocked Label */}
                      {isUnlocked ? (
                        <span className="inline-block mt-1 text-[10px] font-bold text-amber-700 uppercase tracking-widest bg-amber-100/80 px-1.5 py-0.5 rounded">
                          Unlocked
                        </span>
                      ) : (
                        <span className="inline-block mt-1 text-[10px] font-bold text-slate-400 uppercase tracking-widest bg-slate-200/50 px-1.5 py-0.5 rounded">
                          Locked
                        </span>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
            
            {/* Simple gamified footer tracking total progress percentage */}
            <div className="mt-6 pt-5 border-t border-slate-100 text-center">
              <span className="text-xs font-bold text-slate-500 block">Total Badges Earned</span>
              <span className="text-2xl font-black text-slate-800">
                {badgesList.filter(b => b.condition(completedQuests, xp)).length} / {badgesList.length}
              </span>
            </div>
          </div>
        </aside>

      </main>

    </div>
  );
}