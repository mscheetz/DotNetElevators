import type { FloorData } from "../types"

interface FloorCardProps {
  data: FloorData
}

function CountBadge({
  count,
  label,
  vip,
}: {
  count: number
  label: string
  vip?: boolean
}) {
  if (count === 0) return null
  return (
    <span className={`badge ${vip ? "vip" : ""}`}>
      {label} {count}
    </span>
  )
}

export function FloorCard({ data }: FloorCardProps) {
  const up = data.queuedPassengerCount["UP"] ?? 0
  const down = data.queuedPassengerCount["DOWN"] ?? 0
  const upVip = data.queuedVIPCount["UP"] ?? 0
  const downVip = data.queuedVIPCount["DOWN"] ?? 0
  const total = up + down

  return (
    <div className="floor-card">
      <div className="floor-number">
        {data.floorNumber === 10 ? "🏢 " : ""}
        {data.floorNumber}
      </div>
      <div className="floor-counts">
        {total === 0 ? (
          <span className="empty">—</span>
        ) : (
          <>
            <CountBadge count={up} label="▲" />
            <CountBadge count={down} label="▼" />
            <CountBadge count={upVip} label="★▲" vip />
            <CountBadge count={downVip} label="★▼" vip />
          </>
        )}
      </div>
      {data.currentElevatorCount > 0 && (
        <div className="elevator-indicator">
          🛗 {data.currentElevatorCount}
        </div>
      )}
    </div>
  )
}