import { useEffect, useState } from "react"
import { addPassenger, type NewPassengerBody, getInactiveFloors } from "../api"

interface SpawnPassengerModalProps {
  open: boolean
  onClose: () => void
  maxFloor: number
}

export function SpawnPassengerModal({
  open,
  onClose,
  maxFloor,
}: SpawnPassengerModalProps) {
  const [floor, setFloor] = useState(0)
  const [destination, setDestination] = useState(0)
  const [count, setCount] = useState(1)
  const [vip, setVip] = useState(false)
  const [randomizeVip, setRandomizeVip] = useState(false)
  const [sending, setSending] = useState(false)
  const [inactiveFloors, setInactiveFloors] = useState<number[]>([])

  useEffect(() => {
    if (!open) return;

    const fetchInactiveFloors = async () => {
      try {
      const inactiveFloors = await getInactiveFloors();

      setInactiveFloors(inactiveFloors);
      } catch (err) {
        console.error("Error getting inactive floors", err);
      }
    };
    
    fetchInactiveFloors();
  }, [open]);

  if (!open) return null

  function handleCancel() {
    setFloor(0);
    setDestination(0);
    setCount(1);
    setVip(false);
    setRandomizeVip(false);
    onClose();
  }

  async function handleSubmit() {
    setSending(true)
    const body: NewPassengerBody = {
      floor,
      destination,
      passengerCount: count,
      vip,
      randomizeVip,
    }
    await addPassenger(body)
    setSending(false)
    setFloor(0)
    setDestination(0)
    setCount(1)
    setVip(false)
    setRandomizeVip(false)
    onClose()
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Spawn Passenger(s)</h2>
          <button className="modal-close" onClick={onClose}>
            ✕
          </button>
        </div>
        <div className="modal-body">
          <label>
            Source floor
            <select value={floor} onChange={(e) => setFloor(Number(e.target.value))}>
              <option value={0}>Random</option>
              {Array.from({ length: maxFloor }, (_, i) => i + 1)
                .filter((f) => !inactiveFloors.includes(f) && destination !== f)
                .map((f) => (
                  <option key={f} value={f}>
                    Floor {f}
                  </option>
              ))}
            </select>
          </label>
          <label>
            Destination
            <select value={destination} onChange={(e) => setDestination(Number(e.target.value))}>
              <option value={0}>Random</option>
              {Array.from({ length: maxFloor }, (_, i) => i + 1)
                .filter((f) => !inactiveFloors.includes(f) && floor !== f)
                .map((f) => (
                <option key={f} value={f}>
                  Floor {f}
                </option>
              ))}
            </select>
          </label>
          <label>
            Spawn Count
            <input
              type="number"
              min={1}
              max={10}
              value={count}
              onChange={(e) => setCount(Math.max(1, Number(e.target.value)))}
            />
          </label>
          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={vip}
              onChange={(e) => setVip(e.target.checked)}
            />
            VIP
          </label>
          <label className="checkbox-row">
            <input
              type="checkbox"
              checked={randomizeVip}
              onChange={(e) => setRandomizeVip(e.target.checked)}
            />
            Random VIP
          </label>
        </div>
        <div className="modal-footer">
          <button className="action-btn" onClick={handleCancel}>
            Cancel
          </button>
          <button
            className="action-btn primary"
            onClick={handleSubmit}
            disabled={sending}
          >
            {sending ? "Spawning..." : "Spawn"}
          </button>
        </div>
      </div>
    </div>
  )
}