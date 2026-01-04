---

# Percadyn

**A Time-Symmetric Generative Music Sequencer.**

> *Percadyn (Permutation Cellular Automata DYNamics) is a browser-based musical instrument that uses reversible algorithmic physics to weave evolving, non-destructive rhythmic loops.*

## 🎵 The Concept

Most generative music tools are "dissipative"—meaning the system eventually settles into a static state or chaos where the original input is lost.

**Percadyn is different.** It is built on **PERmutation Cellular Automata DYNamics (PCA Dynamics)**.
Unlike standard automata (e.g., Conway's Game of Life), the physics in Percadyn are **bijective** (reversible).

* **Conservation of Data:** "Notes" on the grid are never destroyed, only moved or transformed.
* **Time Symmetry:** The simulation can run forward to generate music, or backward to return exactly to its initial state.
* **The Margolus Neighborhood:** We use a block-based grid logic that alternates phases (Even/Odd steps), allowing individual cells to travel, collide, and interact without ever vanishing.

It is, effectively, a **musical perpetual motion machine**.

## 🚀 Features

* **Visual Sequencer:** A dynamic grid where vertical position represents pitch and horizontal position represents time/step.
* **Reversible Engine:** Scrub the timeline forward or backward without calculation errors.
* **Custom Physics:** Swap between different "Collision Rules" (e.g., Billiard Ball, Tron, Rotation) to change how the melody evolves.
* **Browser-Based:** Runs entirely client-side using Web Audio API.
* **Offline Capable:** Designed as a PWA (Progressive Web App).

## 🛠️ The Tech Stack

This project was designed as a "Deep Dive" into the F# ecosystem, leveraging its strengths in modeling complex domains and state machines.

* **Language:** [F#](https://fsharp.org/) (.NET 8.0)
* **Compiler:** [Fable 4](https://fable.io/) (F# to JavaScript)
* **Architecture:** [Elmish](https://elmish.github.io/) (Model-View-Update, similar to Redux)
* **UI Library:** [Feliz](https://www.google.com/search?q=https://zaid-ajaj.github.io/Feliz/) (React DSL for F#)
* **Bundler:** [Vite](https://vitejs.dev/)
* **Styling:** TailwindCSS

## 📐 Architecture Highlights

Percadyn avoids imperative state mutations. The entire simulation is a pure function.

* **Domain Modeling:** The grid state and rules are modeled using F# **Discriminated Unions**, making invalid states unrepresentable.
* **Pattern Matching:** The "Physics Engine" is a set of pattern matching functions that transform 2x2 blocks of cells.
* **The "Tick":** The application state updates via a deterministic `update` function in the Elmish loop, ensuring the UI (React) is always perfectly synced with the mathematical model.

## 📦 Getting Started

### Prerequisites

* Node.js (LTS)
* .NET 8.0 SDK

### Installation

1. Clone the repository:
```bash
git clone https://github.com/yargnad/Percadyn.git
cd percadyn

```


2. Install dependencies:
```bash
npm install
dotnet tool restore

```


3. Start the development server:
```bash
npm run dev

```


Open `http://localhost:5173` (or the port shown in your terminal).

## 🧪 How to Play

1. **Paint** cells onto the grid to create your initial musical "seed."
2. **Select** a Rule Set (e.g., "Single Rotation").
3. **Press Play.** Watch as the Margolus neighborhood logic permutes your seed into an evolving composition.
4. **Experiment** by drawing "walls" or static reflectors to bounce the sound waves around.

## 🤝 Contributing

This is primarily an educational project for learning F#, but contributions are welcome! If you have ideas for new Permutation Rules or Web Audio synths, feel free to open a PR.

## 📜 License

This project is open-source and available under the AGPL-3.0 License.

---

*Built with F# and Coffee.*
