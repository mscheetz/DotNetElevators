import { useState, useEffect, useCallback, useRef } from "react"
import {
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr"
import type { ElevatorData, FloorData, PassengerData } from "../types"

const HUB_URL = "http://localhost:5000/hubs/building"

export function useSignalR() {
  const [connected, setConnected] = useState(false)
  const [elevators, setElevators] = useState<Map<number, ElevatorData>>(
    () => new Map()
  )
  const [floors, setFloors] = useState<Map<number, FloorData>>(() => new Map())
  const [passengers, setPassengers] = useState<Map<string, PassengerData>>(
    () => new Map()
  )
  const [error, setError] = useState<string | null>(null)
  const connectionRef = useRef<{ stop: () => Promise<void> } | null>(null)

  const onElevatorUpdated = useCallback(
    (data: ElevatorData) =>
      setElevators((prev) => {
        const next = new Map(prev)
        next.set(data.id, data)
        return next
      }),
    []
  )

  const onFloorUpdated = useCallback(
    (data: FloorData) =>
      setFloors((prev) => {
        const next = new Map(prev)
        next.set(data.floorNumber, data)
        return next
      }),
    []
  )

  const onPassengerUpdated = useCallback(
    (data: PassengerData) =>
      setPassengers((prev) => {
        const next = new Map(prev)
        next.set(data.id, data)
        return next
      }),
    []
  )

  useEffect(() => {
    let cancelled = false

    async function start() {
      const connection = new HubConnectionBuilder()
        .withUrl(HUB_URL)
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build()

      connectionRef.current = connection

      connection.on("ElevatorUpdated", onElevatorUpdated)
      connection.on("FloorUpdated", onFloorUpdated)
      connection.on("PassengerUpdated", onPassengerUpdated)

      connection.onreconnecting(() => {
        console.log("SignalR: reconnecting...")
        if (!cancelled) setConnected(false)
      })

      connection.onreconnected((id) => {
        console.log("SignalR: reconnected", id)
        if (!cancelled) setConnected(true)
      })

      connection.onclose((err) => {
        console.log("SignalR: closed", err?.message)
        if (!cancelled) setConnected(false)
      })

      try {
        await connection.start()
        console.log("SignalR: connected")
        if (!cancelled) setConnected(true)
      } catch (err: unknown) {
        const msg = err instanceof Error ? err.message : String(err)
        console.error("SignalR: connection failed", msg)
        if (!cancelled) {
          setConnected(false)
          setError(msg)
        }
      }
    }

    start()

    return () => {
      cancelled = true
      connectionRef.current?.stop()
      connectionRef.current = null
    }
  }, [onElevatorUpdated, onFloorUpdated, onPassengerUpdated])

  return { elevators, floors, passengers, connected, error }
}