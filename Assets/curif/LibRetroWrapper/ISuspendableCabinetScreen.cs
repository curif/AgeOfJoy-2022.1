/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

/// <summary>
/// Implemented by every cabinet screen controller (LibretroScreenController,
/// AGEBasicScreenController, AGEBasicCabinetController) so a cabinet's runtime
/// logic (BT tick, video, running game/program) can be suspended/resumed
/// without SetActive(false)-ing the whole cabinet GameObject.
/// </summary>
public interface ISuspendableCabinetScreen
{
    /// <summary>Stop BT ticking, attract video/audio, and any running game/program.</summary>
    void SuspendAttractAndPlaybackForTransition();

    /// <summary>Resume BT ticking (attract mode) after a suspend.</summary>
    void EnsureAttractLoopRunning();
}
