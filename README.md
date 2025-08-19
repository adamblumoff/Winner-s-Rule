# Winner's Rule (Prototype)

A competitive party game built in Unity.  
Players compete in short microgames. Each round’s winner drafts a temporary rule card that changes how the next round plays. Cards provide an advantage but include drawbacks, forcing strategic choices.

---

## 🎮 Gameplay Loop
1. **Play a microgame** – everyone competes with the same rules.  
2. **Determine a winner** – scores update.  
3. **Winner’s draft** – winner chooses one of three rule cards.  
   - Examples: +8% move speed (drawback: stamina drains faster),  
     +1 shield (drawback: slower respawn),  
     global platform speed +10%.  
4. **Next round** – active rules apply, expired rules clear.  
5. **Final score** – after N rounds, highest score wins.

---

## 🛠 Current Prototype Features
- One microgame: race to the goal.  
- Round system with lobby → game → results → draft → next round.  
- Scriptable rule cards with modifiers and drawbacks.  
- Draft screen for winner choice.  
- Basic scoreboard.

---

## 🚧 Roadmap
- Add 2–3 more microgames (aim, reflex, physics chaos).  
- Expand card pool to 12–15 balanced effects.  
- Local multiplayer polish, networking pass.  
- Juice: sound effects, VFX, better UI.  
- Playtesting and balance iteration.

---

## ▶️ How to Run
- Open project in Unity 2022 LTS or later.  
- Run `MainMenu` scene.  
- Play in editor with 2 players (local inputs or network stub).  

---

## 📄 License
Prototype for testing and feedback only. Not for commercial use yet.
