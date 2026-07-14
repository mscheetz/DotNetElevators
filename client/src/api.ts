import type { ElevatorData, FloorData } from "./types"

const BASE = "http://localhost:5000/api"

export async function getElevators(): Promise<ElevatorData[]> {
  const res = await fetch(`${BASE}/elevators`, {method: "GET"})
  return res.json()
}

export async function toggleElevator(id: number): Promise<boolean> {
  const res = await fetch(`${BASE}/elevators/${id}`, { method: "PUT" })
  return res.ok
}

export async function addElevator(): Promise<number | null> {
  const res = await fetch(`${BASE}/elevators`, { method: "POST" })
  if (!res.ok) return null
  return res.json()
}

export async function getFloors(): Promise<FloorData[]>{
  const res = await fetch(`${BASE}/floors`, { method: "GET" })
  if (!res.ok) return []
  return res.json()
}

export async function toggleFloor(id: number): Promise<boolean> {
  const res = await fetch(`${BASE}/floors/${id}`, { method: "PUT" })
  return res.ok
}

export async function addFloor(): Promise<number | null> {
  const res = await fetch(`${BASE}/floors`, { method: "POST" })
  if (!res.ok) return null
  return res.json()
}

export async function getInactiveFloors() : Promise<number[]> {
  const res = await fetch(`${BASE}/floors/inactive`, { method: "GET" })
  if (!res.ok) return []
  return res.json() as Promise<number[]>  
}

export interface NewPassengerBody {
  floor: number
  destination: number
  passengerCount: number
  vip: boolean
  randomizeVip: boolean
}

export async function addPassenger(body: NewPassengerBody): Promise<boolean> {
  const res = await fetch(`${BASE}/passengers`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  })
  return res.ok
}