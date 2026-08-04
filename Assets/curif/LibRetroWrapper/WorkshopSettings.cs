/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

// Persists which cabinet is currently summoned into the workshop's dev/test slot (position 0,
// room "workshop"), so it survives an app restart. Deliberately kept out of GameRegistry: that
// slot is a disposable dev slot by design (see CabinetsController.IsWorkshopReloadSlot).
public class WorkshopSettings
{
    public string SummonedCabinet;

    public static WorkshopSettings Load()
    {
        return YamlUtils.ParseOptional<WorkshopSettings>(ConfigManager.WorkshopYamlPath);
    }

    public static void Save(string cabinetName)
    {
        YamlUtils.Save(ConfigManager.WorkshopYamlPath, new WorkshopSettings { SummonedCabinet = cabinetName });
    }
}
