import type { ElevatorData } from "../types"

interface ElevatorCardProps {
  data: ElevatorData
}

function DirectionIcon(dir: string) {
  switch (dir) {
    case "UP":
      return "▲"
    case "DOWN":
      return "▼"
    default:
      return "⏸"
  }
}

export function ElevatorCard({ data }: ElevatorCardProps) {
  const dest =
    data.destinationFloor != null ? `→ ${data.destinationFloor}` : "—"

  return (
    <div className={`elevator-card ${data.hasVIPs ? "vip" : ""}`}>
      <div className="elevator-header">
        <span className="elevator-id">🛗 #{data.id}</span>
        {data.hasVIPs && <span className="vip-badge">⭐ VIP</span>}
      </div>
      <div className="elevator-info">
        <div className="info-row">
          <span className="label">Floor</span>
          <span className="value">{data.currentFloor}</span>
        </div>
        <div className="info-row">
          <span className="label">Dir</span>
          <span
            className={`value direction ${data.elevatorDirection.toLowerCase()}`}
          >
            {DirectionIcon(data.elevatorDirection)} {data.elevatorDirection}
          </span>
        </div>
        <div className="info-row">
          <span className="label">Dest</span>
          <span className="value">{dest}</span>
        </div>
        <div className="info-row">
          <span className="label">Pass</span>
          <span className="value">{data.passengerCount}</span>
        </div>
      </div>
    </div>
  )
}