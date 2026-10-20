# 🩸 Sunday's Last Breath
### Technical Narrative Design & Systems Design Portfolio
**Author:** Elang Esa (Neal Sage) Yudhistira  
**Role:** Technical Narrative Designer | Game Writer & Systems Designer  
**Format:** Interactive Narrative Thriller & Survival RPG  
**Engine & Tech Stack:** Twine (SugarCube 2 / JavaScript) & Unity (C# Hybrid 2.5D Survival RPG)  
**Target Recruiter Profile:** Narrative Designers, Game Designers, Systems Designers (e.g., Kaigan Games, Inkle, Supermassive, Red Hook)

---

> 📌 **How to Use in Notion:**  
> In Notion, click **Import** in the left sidebar, select **Markdown**, and choose this file (`Notion_Portfolio_Sunday_Last_Breath.md`). Notion will automatically parse every section, heading, table, quote callout, and code block into a complete, beautifully structured native Notion Portfolio page!

---

## 🧭 Executive Summary & Game Pitch

### The Logline
> *At 23:45 on Saturday, October 25, 2025, an emergency broadcast orders Seattle to barricade inside. By midnight, power grids collapse under an 87% pathogen outbreak. You have six hours before the military executes Operation WILDFIRE—a total thermal firebombing purge of the metropolitan core. Every door you breach, clue you read, and passenger you squeeze into your four-seat sedan changes who lives, who dies, and how much of your humanity survives until dawn.*

### Core Design Pillars
1. **Diegetic Found-Interface Storytelling:** Worldbuilding, dread, and puzzle codes are discovered naturally through cracked smartphone social feeds (`#SeattleLockdown`), analog police scanner intercepts (`154.790 MHz`), encrypted military terminals (`SYS_ADMIN - SECURE_NODE_04`), maintenance logbooks, and thermal gas pump receipts.
2. **5-Tier Obstacle Resolution ("No Dead Ends, Always a Price"):** Obstacles never block player progress behind a single binary check. Every locked door or crisis supports up to 5 resolution vectors: **Hard Knowledge Clue → Profession/Trait → Skill/Tool → Raw Stat → Dynamic Stat-Scaled QTE Minigame**.
3. **Scarcity of Space & Empathy:** Survival is bound by mechanical constraints. The player's getaway sedan has **4 physical seats**. Rescuing **Vane** (1 seat), **Maya & Lily** (2 seats), or **Diane & her two boys** (3 seats) forces genuine triage over who can be saved.
4. **Psychological Morality (`Humanity 0–100`):** Replaces binary "good/evil" meters with internal psychological drift. Humanity dynamically alters the protagonist's internal monologue and available spoken dialogue across three distinct brackets: **Empathetic**, **Pragmatic**, and **Predatory**.
5. **Living Chronology & World Mutation:** Taking long mid-journey detours advances the global clock (`_arrivedLate`), fundamentally altering Act 3's environment from a pitch-black midnight downpour to a freezing gray dawn, replacing fresh coffee with stale grounds and extinguishing candles.

---

## 📖 Section 1: Narrative Bible & Story Breakdown

### Setting the Clock: The First 12 Hours
* **21:46 (Sat, Oct 25):** King County Sheriff issues Civil Danger Warning.
* **23:45 (Prologue Start):** Player awakens in **Unit 404 of The Ascott Residency**, 15 minutes before the midnight power cut.
* **00:00 (Midnight — Act 1):** Grid dies; infected breaches erupt across the 4th floor.
* **01:30 – 03:30 (Act 2 & 2.5):** Street riot escape, I-5 Southbound gridlock, Route 9 refueling, and divergent routes (Rural Route 11 vs. Oakhaven Cabin).
* **04:00 (Night Arrival) OR 06:15 (Dawn Arrival — Act 3):** Outskirts of Timberline, Sheriff Substation 4, and the final bastion at Timberline Regional High School before the military firebombs Sector 4.

---

### Act-by-Act Synopsis

#### 🚪 Prologue: The Final Fifteen Minutes (`Unit 404`)
The player awakens drenched in sweat in their downtown apartment. Sirens wail through the rain-streaked glass. The protagonist's internal thoughts immediately react to their chosen **Profession** and **Traits** (e.g., a *Police Officer* identifies a Code-3 dispatch breakdown; a *Paranoid* survivor notes sirens are fleeing intersections rather than clearing them).

The player has 15 minutes (3 actions, 5 minutes each) before midnight:
* **The Cracked Smartphone:** Accesses `#SeattleLockdown`. Contrasts municipal cover-up tweets with raw citizen video footage. Contains a crucial text from neighbor **Prof. Arthur Harmon** providing his door keypad PIN (`8445`).
* **The Analog Police Scanner:** Intercepts `154.790 MHz`. Officers are overrun; rules of engagement are officially suspended with orders to *"aim for the head."*
* **The Encrypted Terminal:** Hacks `SYS_ADMIN - SECURE_NODE_04`, discovering **Operation WILDFIRE**—the military's scheduled 06:00 thermal firebombing purge of Seattle.

#### 🏢 Act 1: The Ascott Residency
At midnight, violent banging rattles the corridor. The player enters the 4th-floor hallway hub:
* **Unit 408 (Prof. Harmon):** Keypad secured. Can be opened using PIN `8445`, reading finger grease (`PER 6`), lockpicking, or hacking. Uncovers research on the pathogen's neural incubation.
* **Unit 405 (Marcus's Sanctuary):** Rigged with a lethal shotgun tripwire. Disarming it rewards a **Topographical Map** (unlocking Act 2.5 Oakhaven Cabin), Marcus's **Leather Journal**, and his custom **Hunting Knife** (both crucial for negotiating with his wife in Act 2.5).
* **Unit 402 ("The Quiet Room"):** Chopin's *Nocturne* plays on vinyl amidst the smell of bleach. An elderly couple took their own lives after reading a false online rumor about military nerve gas. Yields an unfired `.38 Revolver`.
* **Unit 401 ("The Party That Ended"):** Upbeat pop music loops over spilled drinks. A partygoer is pinned under a 300-lb oak bookshelf holding **Sedan Keys**. Requires `STR 6`, `Construction Worker`, `Crowbar`, or a QTE.
* **Utility Closet:** A heavy steel door hides the building janitor's final logbook, detailing his spiking fever and joint pain before locking himself in.
* **Stairwell Descent:** Descending silently requires light or patience. Sprinting in the dark risks tripping (`$made_noise = true`), which causes the infected tenant on Floor 2 to immediately sprint-attack when the fire door opens.
* **Lobby Climax:** Four simultaneous crises unfold:
  1. **Vane:** Cornered by an infected, defending with a camera case. Saving him recruits him as a companion.
  2. **Maya & Lily:** Trapped behind the reception desk, preparing to run into the lethal police crossfire.
  3. **Dead Soldier's Pack:** High-tier loot, but taking it while others scream costs `-10 Humanity`.
  4. **The Police Barricade:** Crumbling under the infected horde outside.

#### 🛣️ Act 2: Urban Collapse & The Highway
* **Alley Ambush:** Fleeing the collapsing barricade through the back alleyway. Fighting an infected jumper saves stragglers; fleeing costs `-5 Humanity`.
* **Parking Lot Getaway:** If Vane was rescued in Act 1, he hot-starts the sedan and rams the boss-level infected mechanic at 40 mph, saving the player.
* **The Emotional Anchor (`Act2_TheRoad`):** As Seattle burns in the rearview mirror, the player defines who they are surviving for (Partner, Parents, or Lone Wolf). This dynamically alters the story's climax in Act 3.
* **I-5 Gridlock & Route 9 Gas Station:** The highway is blocked by abandoned vehicles. The player diverts to a rural gas station. Rather than forcing the locked pump, the player can search dark aisles to find a thermal receipt with PIN `6821`.

#### 🌲 Act 2.5: Divergent Midpoint Branches
* **Branch A — Rural Route 11 (`Act2_5_MayaCrash`):** The player discovers a crashed police cruiser wrapped around a tree with its horn blaring. The rookie cop who escorted Maya and Lily has turned into an infected host, snapping at them through the partition. The player can:
  * *Heroically Rescue them (+10 Humanity):* Pry the doors, cut the battery, or use Vane as a distraction. Maya & Lily join the party (taking 2 car seats).
  * *Extort them (-25 Humanity):* Demand supplies through the window crack before helping. Maya joins with bitter hatred.
  * *Abandon them (-40 Humanity):* Drive away. Permanently breaks Vane's trust (`$vane_trust = "broken"`).
* **Branch B — Oakhaven Cabin (`Act2_5_MarcusCabin`):** Accessible only if Marcus's map was scavenged in Unit 405. The player is held at shotgun-point by Marcus's wife, Diane. Presenting Marcus's Hunting Knife (`+15 Humanity`) or Journal (`+10 Humanity`) proves his fate, de-escalating the standoff. Diane and her two boys join the party (taking 3 car seats). Taking this mountain detour advances the global timeline past dawn (`_arrivedLate = true`).

#### 🏫 Act 3: Timberline Outskirts & Sanctuary
* **The Colonial House & Sheriff Substation 4:** Depending on arrival time, the house is either cloaked in midnight rain with a burning candle and warm coffee, or pale dawn frost with cold grounds and burned-out wax.
* **The 4-Seat Sedan Economy:** Meeting Officer Reyes triggers a capacity check. If the player is carrying Vane and Maya/Lily, or Diane's family, the car is full (`_carCount >= 4`), forcing Reyes to coordinate an alternate tactical route.
* **Timberline Regional High School:** Fortified by Matriarch Evelyn and Commander Hayes.
  * *Dynamic Anchor Twist:* If the player chose `Parents` in Act 2, **Matriarch Evelyn is revealed to be the player's mother**, transforming the refugee intake into an emotional reunion.
  * *Partner Anchor:* If the player chose a partner, they learn their partner is trapped at a nearby 7-Eleven.
* **Climax: The 7-Eleven Operation:** A multi-phase tactical rescue operation alongside Officer Reyes and companions to secure medical supplies and extract trapped survivors before the infected swarm overruns the valley.

---

## ⚙️ Section 2: Technical Narrative Systems & State Architecture

### 1. The 5-Tier Obstacle Resolution Architecture
To eliminate frustrating "dead ends" while maintaining high stakes, every major lock or crisis provides five distinct resolution vectors:

```
                  ▲
                 / \
                / T1 \      Tier 1: Diegetic Knowledge Clue (0 Cost, 0 Noise)
               /------\
              /   T2   \    Tier 2: Profession / Trait Bypass (Silent)
             /----------\
            /     T3     \  Tier 3: Skill Check / Inventory Tool (Fast)
           /--------------\
          /       T4       \Tier 4: Raw Attribute Threshold (High Stamina)
         /------------------\
        /         T5         \ Tier 5: Stat-Scaled Dynamic QTE Fallback (Risk)
       ------------------------
```

#### Obstacle Implementation Table

| Location & Obstacle | Tier 1: Diegetic Clue | Tier 2: Profession / Trait | Tier 3: Skill / Tool | Tier 4: Attribute | Tier 5: QTE / Fallback |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Unit 408 Electronic Keypad** | Code `8445` (SMS in `Prologue_Phone`) | `Burglar` or `LOCKSMITH` | `Tech >= 2` or `Multi-Tool` | `PER >= 6` (Finger smudges on 4, 5, 8) | `STR` Brute Force / 3-Strike Lockout |
| **Unit 405 Shotgun Tripwire** | — | `Burglar` or `LOCKSMITH` | `Tech >= 2` or `Multi-Tool` | `PER >= 6` (Trace wire) | Dynamic Agility QTE |
| **Unit 401 300-lb Oak Shelf** | — | `Construction Worker` / `BREACHER` | `Crowbar` or `Tire Iron` | `STR >= 6` (Deadlift) | Dynamic Strength QTE (`-10 HP` on fail) |
| **4th Floor Utility Door** | — | `Burglar` (Manual cylinder) | `Tech >= 2` (Short mag-lock) | `STR >= 7` (Shoulder slam) | Dynamic Strength QTE |
| **Route 9 Gas Station Pump** | PIN `6821` (Store thermal receipt) | `IT Specialist` | `Tech >= 2` (Pump relay) | — | Manual siphon (Exhaustion + Ambush) |
| **Maya's Crashed Police Cruiser** | — | `HOTWIRE` (Cut battery cable) | `Crowbar` / Companion Vane | — | Combat (`-10 Stamina`, kill cop) |

#### Dynamic Stat-Scaled QTE Formula
When a player lacks the required attribute or tool, the minigame calculates their attribute deficit:
```javascript
// Mathematical scaling ensures builds are rewarded without hard-locking other playstyles
let statDiff = targetStat - playerStat;
let qteSpeed = 2.2 + (statDiff * 0.9);
let qteWidth = 25 - (statDiff * 4);
Minigame_QTE(qteSpeed, qteWidth);
```

---

### 2. The Psychological Humanity System (`$hum`: 0–100)
Humanity is not a binary alignment bar; it is an internal psychological state that alters the protagonist's voice and perception:

* **High Humanity (`$hum >= 70` — The Empathetic Protector):**
  * *Mindset:* Driven by protective instincts and survivor guilt.
  * *Dialogue Tone:* Reassuring, de-escalating, and empathetic.
* **Mid Humanity (`$hum 31–69` — The Pragmatic Survivor):**
  * *Mindset:* Realistic risk assessment; balances morality against ammo and stamina.
  * *Dialogue Tone:* Guarded, concise, and transactional.
* **Low Humanity (`$hum <= 30` — The Ruthless Predator):**
  * *Mindset:* Darwinian detachment; other humans are liabilities or resources.
  * *Dialogue Tone:* Coercive, cynical, and intimidating.

---

### 3. The 4-Seat Vehicle Economy (`_carCount`)
Companions are not abstract inventory items. They occupy physical seats in the player's sedan:

```javascript
/* Dynamic Seat Calculation */
let carCount = 1;                              // Player (Driver)
if (vane_status === "joined")  carCount += 1;  // Vane (Passenger)
if (maya_status === "joined")  carCount += 2;  // Maya + 7yo Lily
if (diane_status === "joined") carCount += 3;  // Diane + 2 Boys
let carFull = (carCount >= 4);
```
When meeting **Officer Reyes** in Act 3, `_carFull` dictates whether he can be evacuated inside the vehicle or if a high-risk perimeter rendezvous is necessary.

---

### 4. Living Chronology & Environmental Mutation (`_arrivedLate`)
Visiting Oakhaven Cabin takes hours, toggling `_arrivedLate = true`. This alters the entire aesthetic and scavenging balance of Act 3:

| Environmental Detail | Night Arrival (`_arrivedLate == false` — 04:00 AM) | Dawn Arrival (`_arrivedLate == true` — 06:15 AM) |
| :--- | :--- | :--- |
| **Visual Atmosphere** | Pitch-black downpour, flashes of distant lightning | Pale gray morning mist, frost-covered grass |
| **Porch Window** | Thick blanket glows from an active candle inside | Amber candle burned down to a dead puddle of wax |
| **Smell & Sensory** | Freshly brewed coffee aroma under the door | Stale, cold coffee grounds from utility exhaust |
| **Scavenging Yield** | 2× Canned Food, 2× Water, 2× Painkillers | 1× Canned Food, 1× Water, 1× Painkillers (pre-looted) |
| **Companion Reaction** | Maya worries Lily will catch hypothermia in the rain | Maya warns the lifting morning fog leaves their car exposed |

---

## 📱 Section 3: Diegetic Media & Environmental Storytelling

### The 3 Core Found Interfaces (Kaigan Games Showcase)

#### 1. The Cracked Smartphone Social Feed (`#SeattleLockdown`)
* **Municipal Deception vs. Ground Reality:** Official city channels attempt to suppress panic by reporting a "water main rupture at 4th & Pike," while citizen video clips show infected civilians ignoring tear gas and overrunning riot police.
* **Actionable Puzzle Integration:** A personal text from **Prof. Harmon** provides the exact code (`8445`) to enter his apartment safely.

#### 2. Analog Police Scanner Intercept (`FREQ: 154.790 MHz`)
* **Audio-Text Intercept:** Discloses the pathogen's physiological resistance to sedatives and body shots.
* **Narrative Turning Point:** Central dispatch authorizes lethal force and orders officers to *"aim for the head,"* setting up why authorities outside the Ascott lobby are firing on sight.

#### 3. Encrypted Military Terminal (`SYS_ADMIN - SECURE_NODE_04`)
* **Directive OPR_WILDFIRE:** Breaching this node reveals an 87.4% saturation forecast and reveals that the entire sector will be firebombed at 06:00 sharp, establishing the macro ticking clock of the story.

---

## 👥 Section 4: Characters & Relationship Matrix

### Character Profiles

```
               [ The Survivor (Player Character) ]
                                |
        +-----------------------+-----------------------+
        |                       |                       |
     [ Vane ]            [ Maya & Lily ]         [ Diane & Sons ]
(Digital Creator)      (Disillusioned Aunt)    (Survivalist Family)
  1 Car Seat              2 Car Seats             3 Car Seats
  • Front-seat buddy      • Moral compass         • Standoff resolution
  • Car-ram mechanic      • Weapon-reactive       • Marcus heirloom link
        |                       |                       |
        +-----------------------+-----------------------+
                                |
                    [ Officer Reyes & Substation 4 ]
                                |
                    [ Matriarch Evelyn & Hayes ]
                     (Dynamic Parent/Anchor Twist)
```

* **The Survivor (Player Character):** Defined by 6 Professions and 15 Traits with custom internal monologue for every archetype.
* **Vane:** An androgynous digital content creator who used his camera case as a bludgeon. Brings self-aware wit and tactical driving support. If the player abandons Maya and Lily, Vane's trust breaks permanently (`$vane_trust = "broken"`).
* **Maya & Lily:** Maya trusted the badge, but the infected officer crashed their car into a tree. Dynamically reacts to the player's equipped weapon (`_isGun`, `_isBlade`, `_isBlunt`).
* **Diane & Marcus (Two-Half Arc):** Marcus is encountered as an environmental ghost in Act 1; Diane is encountered in an armed standoff in Act 2.5. Bringing Marcus's knife or journal bridges the narrative across two acts.
* **Officer Reyes:** A disciplined, wounded deputy holding Substation 4. Serves as the tactical bridge to the school sanctuary.
* **Matriarch Evelyn:** Sanctuary commander. If the player selected `Parents` as their emotional anchor in Act 2, Evelyn is revealed to be the player's mother.

---

## 📝 Section 5: Curated Writing Samples

### Sample 1: The Disinformation War & Diegetic SMS
*(From `Prologue.twee` & `Prologue_Phone.twee`)*

```text
[ EMERGENCY ALERT SYSTEM — KING COUNTY SHERIFF'S OFFICE ]
ISSUED: OCT 25, 2025 — 21:46 PST
CIVIL DANGER WARNING — MANDATORY SHELTER-IN-PLACE
```

You wake up choking on dry air, sheets tangled around your legs like wet rope. Outside the rain-streaked glass of Unit 404, the city is screaming. Not a single siren, but a dozen of them, overlapping in a ragged chorus through the concrete canyons of downtown Seattle.

> **[IF TRAIT == "PARANOID"]**  
> *You don't need to look out the window to know the pattern is wrong. Sirens respond to a scene and stop. These aren't stopping. They're circling, fleeing intersections they were supposed to hold.*

You reach for your phone. Battery: `14%`. Signal: `1 BAR (ROAMING)`.

* **`@CityScanner_WA` (Verified Municipal):** `[ADVISORY]` Water main rupture and transformer fire near 4th & Pine. Remain indoors. Keep 911 clear.
* **`@anon_9921`:** *WTF IS HAPPENING ON PIKE?! The cops opened fire and they didn't even slow down!! [Attached Video: 0:08 — Shaky footage through rain. Blue strobes illuminate riot police falling back as three unarmored civilians sprint through tear gas, tackling an officer.]*
* **SMS from Prof. Arthur Harmon (Unit 408):** *"If you're still in the building, DO NOT go down to the street. The police aren't evacuating anyone—they're containing us. If I don't make it back to 408 by midnight, my door code is 8445. Take the research binder off my desk and get out of the city before dawn."*

---

### Sample 2: Environmental Tragedy — Unit 402 ("The Quiet Room")
*(From `Unit_402.twee`)*

The door to Unit 402 isn't latched. It yields to your fingertips, swinging inward without a sound.

The first thing that hits you isn't the smell of blood. It is the clinical sting of household bleach, layered beneath the warm, dusty crackle of a vinyl record spinning on a turntable.

Chopin's *Nocturne in E-flat major* drifts softly through the dim room—gentle, unhurried, and devastatingly out of place.

On the floral-patterned sofa sit Mr. and Mrs. Anderson in their Sunday best. Mrs. Anderson's head rests peacefully against her husband's shoulder. Their fingers are intertwined on his knee. Neither of them is breathing.

On the glass coffee table sit two empty crystal tumblers, a half-empty jug of bleach, and a printed screenshot from an internet conspiracy forum weighted down by a brass coaster:

> **`LEAKED PENTAGON MEMO: MILITARY TO PUMP NERVE GAS INTO SEATTLE HIGH-RISES AT MIDNIGHT TO CONTAIN RIOTS — NO SURVIVORS.`**

Beside the printout rests a blued-steel `.38 Revolver`, fully loaded, its cylinder never turned. Mr. Anderson couldn't bear the sound of a gunshot in their home. They died trying to save each other from a lie on a screen.

---

### Sample 3: High-Tension Lobby Triage
*(From `Lobby_Entrance.twee`)*

You burst through the fire doors into the Grand Lobby, and the acoustic violence hits you like a physical blow. The glass facade facing the street has been shattered inward. Freezing rain and flashing blue police strobes slice through the dark:

* **To your left, by the brass elevators:** An androgynous twenty-something in a torn designer jacket (**Vane**) is pinned against the steel doors, swinging a heavy Pelican camera case like a battering ram against a snarling former tenant. *"Get off me! Someone—please!"*
* **To your right, behind the reception desk:** A woman (**Maya**) presses a terrified seven-year-old girl (**Lily**) into the cabinetry, preparing to sprint across the glass toward the flashing police cruisers outside.
* **In the center of the terrazzo floor:** Beside an overturned armchair lies a dead National Guardsman. Strapped to his webbing is an untouched **Military Field Pack** bulging with sealed rations and ammo.
* **Outside the shattered entrance:** Two police officers are firing into the crowd. The younger officer is sweating violently, pressing a blood-soaked rag against a deep bite on his forearm.

You have seconds before the perimeter falls. **Who—or what—do you prioritize?**

---

### Sample 4: Humanity-Gated Standoff & Knife Payoff
*(From `Act2_5_MarcusCabin.twee`)*

The porch light snaps on. The timber door of Oakhaven Cabin bangs open. Diane steps onto the wet cedar boards, leveling a double-barreled shotgun squarely at your chest. Behind her legs, two young boys peer out, shivering.

*"Stop right there!"* Diane's voice cracks over the thunder. *"Take one more step and I'll paint the mud with you! Who are you?! Where is Marcus?!"*

#### Dynamic Spoken Dialogue (Humanity Gated)
* **[High Humanity: $hum >= 70]**  
  > **You:** *"Easy! Keep the gun on me if it makes you feel safe, Diane, just don't pull that trigger in front of your boys. My hands are up. I came from the Ascott apartments—Marcus sent me."*
* **[Mid Humanity: $hum 31–69]**  
  > **You:** *"Whoa. Hold your fire. Look at me—I'm soaking wet, I'm not biting, and if I wanted to rob you I wouldn't have walked up your front steps in the light. I found your cabin on a map in Unit 405."*
* **[Low Humanity: $hum <= 30]**  
  > **You:** *"Put the twelve-gauge down before you break your own collarbone with it, lady. Look at your hands—you're shaking so hard you'll miss my chest and blow a hole in your roof. Listen to what I have to say about your husband."*

#### The Act 1 Item Payoff
* **[IF PLAYER HAS "Hunting Knife" FROM UNIT 405 — +15 HUMANITY]**  
  > Slowly, using two fingers, you draw Marcus's custom hunting knife by the sheath and set it on the wet bottom step. The porch light catches the brass bolster and the initials carved into the walnut grip.  
  > **You:** *"He rigged Unit 405 to keep scavengers out while he packed for you. He didn't make it out of Seattle, Diane. He left this knife beside the map so someone would get you and the boys out before dawn."*  
  > Diane stares down at the walnut handle. A choked sob escapes her, and the twin barrels slowly sink toward the floorboards.

---

### Sample 5: Companion Backseat Banter & Environmental State
*(From `Act3_Home.twee`)*

#### Night Arrival (`_arrivedLate == false`) vs. Dawn Arrival (`_arrivedLate == true`)

> **[Night Arrival — 04:00 AM]**  
> Every window intact, every shutter on its hinges. In a neighborhood of kicked-in doors, this is conspicuous. The ground-floor window to the left of the porch has been covered from inside with a dark blanket. But a faint amber glow pulses behind it. A candle. Someone is holding the dark at bay.

> **[Dawn Arrival — 06:15 AM]**  
> In the pale gray of early morning, the house looks almost normal. The window blanket is tucked carefully at the corners. Along the bottom edge: a faint amber warmth. A candle burning in the middle of the morning. Either no one told them it was daylight, or they never went to sleep.

#### Combinatorial Backseat Banter (Vane + Maya)
* **Vane (if Night):** *"Candle in the window. Covered from inside. They're keeping a low profile in the dark. Keep your eyes on the tree line, please."*
* **Vane (if Dawn):** *"A candle burning in broad daylight? It's a signal... or they've been awake all night and passed out."*
* **Maya (if Night + Player has a Firearm):** *"It's freezing. Lily is shivering violently. If there's heat in there, I don't care who owns it. Just... keep your finger off the trigger unless they force your hand. Talk first."*
* **Maya (if Dawn):** *"The mist is lifting. We're completely exposed out here on the curb. If we're going inside, do it before whoever is in there wakes up."*
* **You:** *"Stay in the car with Lily. Doors locked. Vane, you're on the porch with me. We assess before we confront."*

---
*Sunday's Last Breath — Narrative & Systems Portfolio by Elang Esa (Neal Sage) Yudhistira*

