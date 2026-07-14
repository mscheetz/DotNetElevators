import { useSignalR } from "../hooks/useSignalR"
import { FloorPanel } from "./FloorPanel"
import { ElevatorPanel } from "./ElevatorPanel"

export function BuildingDashboard() {
  const { elevators, floors, connected, error } = useSignalR()

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>Building Dashboard</h1>
        <span className={`status ${connected ? "connected" : "disconnected"}`}>
          {connected ? "● Connected" : "○ Disconnected"}
        </span>
      </header>
      {error && <div className="error-banner">Connection error: {error}</div>}
      <div className="dashboard-body">
        <FloorPanel floors={floors} />
        <ElevatorPanel elevators={elevators} />
      </div>
    </div>
  )
}