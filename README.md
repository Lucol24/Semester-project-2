# 🔥 Semester Project 2: **Danfoss** Heating Optimization  

This is the **Group 3** repository for Semester Project 2. Below is the structure and file distribution for our **Danfoss** heating project.  

---

## 📁 **Project Structure**  
```plaintext
SEMESTER-PROJECT-2/
├── 🏗️ bin/                          
├── 📂 Data/                          # Contains system-related datasets
│   ├── 📄 heat_demand.csv           
│   ├── 📄 production_units.json      
│   │
├── ⚙️ obj/                           
├── 📦 Source/                        # Core source files
│   ├── 🏢 AssetManager/              # Asset Manager module
│   │   ├── 📜 AssetManager.cs        
│   │   ├── 🏭 ProductionUnit.cs    
│   │   │
│   ├── 🔍 SourceDataManager/         # Source Data Manager module
│   │   ├── 🔥 HeatDemand.cs          
│   │   ├── 📊 SourceDataManager.cs   
│   │   │
│   ├── ✔️ Tests/                     # Xunit tests for all modules
│   │   ├── 📝 AssetManagerTests.cs  
│   │   ├── 📝 SourceDataManagerTests.cs 
│   │   │
│   ├── 🚀 Program.cs                 
│   │
├── 📄 .gitignore                     
├── 📘 README.md                      
├── 🔧 Semester-project-2.sln        
└── 🔧 Semester-project-2.csproj 
```

---

## 📜 **Key Components**  

### **📂 Data Files**  
- 📄 **`heat_demand.csv`** – Stores heating demand data for analysis.  
- 📄 **`production_units.json`** – Contains details about production units, such as energy production, consumption, and costs.  


### 📦 Source Files
**`🏢 AssetManager`**  
- 📜 **`AssetManager.cs`** – Manages and loads static system data, including production units and heat demand from JSON and CSV files.  
- 🏭 **`ProductionUnit.cs`** – Defines the `ProductionUnit` class, which stores data about energy production units, including max heat, electricity, costs, and emissions.

**`🔍 SourceDataManager`**  
- 🔥 **`HeatDemand.cs`** – Defines the `HeatDemand` class, which stores winter and summer heat demand records.
- 📊 **`SourceDataManager.cs`** – Handles dynamic data processing and ensures efficient management of system-related information.

**`✔️ Tests`**  
- 📝 **`AssetManagerTests.cs`** – Validates JSON file loading, error handling, and production unit data display.
- 📝 **`SourceDataManager.cs`** – 

- 🚀 **`Program.cs`** – Main application logic and execution flow.
---

## 👥 **Contribution Guidelines**  
💡 When contributing, make sure your comments are clear, and your changes are isolated to your assigned area. This helps avoid conflicts and ensures smooth integration of everyone’s work.  

---
