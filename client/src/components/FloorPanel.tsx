import type { FloorData } from "../types"
import { FloorCard } from "./FloorCard"

interface FloorPanelProps {
  floors: Map<number, FloorData>
}

export function FloorPanel({ floors }: FloorPanelProps) {
  const sorted = [...floors.values()].sort(
    (a, b) => b.floorNumber - a.floorNumber
  )

  return (
    <div className="floor-panel">
      <h2>Floors</h2>
      <div className="floor-list">
        {sorted.length === 0 && (
          <div className="empty-state">Waiting for data...</div>
        )}
        {sorted.map((f) => (
          <FloorCard key={f.floorNumber} data={f} />
        ))}
      </div>
    </div>
  )
}