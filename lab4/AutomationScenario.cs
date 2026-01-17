using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeSystem
{
    public class AutomationScenario
    {
        public string ScenarioId { get; private set; }
        public string Name { get; set; }
        public string TriggerCondition { get; set; }
        private List<ScenarioAction> actions;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; private set; }

        public AutomationScenario(string scenarioId, string name, string triggerCondition)
        {
            ScenarioId = scenarioId;
            Name = name;
            TriggerCondition = triggerCondition;
            actions = new List<ScenarioAction>();
            IsActive = true;
            CreatedDate = DateTime.Now;
        }

        public void AddAction(ScenarioAction action)
        {
            if (!actions.Contains(action))
            {
                actions.Add(action);
            }
        }

        public void RemoveAction(ScenarioAction action)
        {
            actions.Remove(action);
        }

        public List<ScenarioAction> GetActions()
        {
            return new List<ScenarioAction>(actions);
        }

        public int GetActionCount()
        {
            return actions.Count;
        }

        public void Activate()
        {
            IsActive = true;
            Console.WriteLine($"[SCENARIO] Сценарий '{Name}' активирован");
        }

        public void Deactivate()
        {
            IsActive = false;
            Console.WriteLine($"[SCENARIO] Сценарий '{Name}' деактивирован");
        }

        public void Execute()
        {
            if (!IsActive)
            {
                Console.WriteLine($"[SCENARIO] Сценарий '{Name}' неактивен");
                return;
            }

            Console.WriteLine($"[SCENARIO] Выполнение сценария '{Name}'...");
            foreach (var action in actions)
            {
                action.Execute();
            }
            Console.WriteLine($"[SCENARIO] Сценарий '{Name}' выполнен");
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"┌────────────────────────────────────────");
            Console.WriteLine($"│ Сценарий: {Name}");
            Console.WriteLine($"│ ID: {ScenarioId}");
            Console.WriteLine($"│ Условие: {TriggerCondition}");
            Console.WriteLine($"│ Статус: {(IsActive ? "АКТИВЕН" : "НЕАКТИВЕН")}");
            Console.WriteLine($"│ Действий: {actions.Count}");
            Console.WriteLine($"└────────────────────────────────────────");
        }

        public void DisplayActions()
        {
            Console.WriteLine($"\nДействия сценария '{Name}':");
            if (actions.Count == 0)
            {
                Console.WriteLine("  Действий нет.");
                return;
            }

            foreach (var action in actions)
            {
                action.DisplayInfo();
            }
        }

        public string Serialize()
        {
            string actionIds = string.Join(",", actions.Select(a => a.ActionId));
            return $"{ScenarioId}|{Name}|{TriggerCondition}|{IsActive}|{actionIds}";
        }

        public static AutomationScenario Deserialize(string data)
        {
            var parts = data.Split('|');
            if (parts.Length != 5) return null;

            return new AutomationScenario(
                parts[0],
                parts[1],
                parts[2]
            )
            {
                IsActive = bool.Parse(parts[3])
            };
        }

        public override string ToString()
        {
            return $"{Name} ({actions.Count} действий)";
        }
    }
}