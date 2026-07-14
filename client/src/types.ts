export interface ElevatorData {
  id: number
  currentFloor: number
  destinationFloor: number | null
  elevatorDirection: string
  passengerCount: number
  hasVIPs: boolean
  isActive: boolean
}

export interface FloorData {
  floorNumber: number
  queuedPassengerCount: Record<string, number>
  queuedVIPCount: Record<string, number>
  currentElevatorCount: number
  isActive: boolean
}

export interface PassengerData {
  id: string
  status: string
  elevatorId: number | null
  floorNumber: number | null
  vip: boolean
  direction: string
}