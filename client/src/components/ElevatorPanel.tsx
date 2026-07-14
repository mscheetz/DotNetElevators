import type { ElevatorData } from "../types"
import { ElevatorCard } from "./ElevatorCard"

interface ElevatorPanelProps {
  elevators: Map<number, ElevatorData>
}

export function ElevatorPanel({ elevators }: ElevatorPanelProps) {
  const sorted = [...elevators.values()].sort((a, b) => a.id - b.id)

  return (
    <div className="elevator-panel">
      <h2>Elevators</h2>
      <div className="elevator-grid">
        {sorted.length === 0 && (
          <div className="empty-state">Waiting for data...</div>
        )}
        {sorted.map((e) => (
          <ElevatorCard key={e.id} data={e} />
        ))}
      </div>
    </div>
  )
}