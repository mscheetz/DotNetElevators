import { useState } from "react"
import { useSignalR } from "../hooks/useSignalR"
import { FloorPanel } from "./FloorPanel"
import { ElevatorPanel } from "./ElevatorPanel"
import { SpawnPassengerModal } from "./SpawnPassengerModal"

export function BuildingDashboard() {
  const { elevators, floors, connected, error } = useSignalR()
  const [showSpawn, setShowSpawn] = useState(false)

  const maxFloor = [...floors.keys()].reduce(
    (max, f) => Math.max(max, f),
    0
  )

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>Building Dashboard</h1>
        <div className="header-actions">
          <button className="action-btn" onClick={() => setShowSpawn(true)}>
            + Passenger
          </button>
          <span
            className={`status ${connected ? "connected" : "disconnected"}`}
          >
            {connected ? "● Connected" : "○ Disconnected"}
          </span>
        </div>
      </header>
      {error && <div className="error-banner">Connection error: {error}</div>}
      <div className="dashboard-body">
        <FloorPanel floors={floors} />
        <ElevatorPanel elevators={elevators} />
      </div>
      <SpawnPassengerModal
        open={showSpawn}
        onClose={() => setShowSpawn(false)}
        maxFloor={maxFloor}
      />
    </div>
  )
}