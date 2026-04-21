import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { createTask } from '../api/tasks'
import type { TaskType } from '../types/task'

export function TaskForm() {
    const queryClient = useQueryClient()
    const [type, setType] = useState<TaskType>('Simulation')
    const [maxAttempts, setMaxAttempts] = useState(3)

    const mutation = useMutation({
        mutationFn: () => createTask(type, maxAttempts),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['tasks'] })
        }
    })

    return (
        <div className="card bg-base-100 shadow p-6">
            <h2 className="text-xl font-bold mb-4">Create Task</h2>
            <div className="flex gap-4 items-end">
                <select 
                    className="select select-bordered"
                    value={type} 
                    onChange={e => setType(e.target.value as TaskType)}
                >
                    <option value="Simulation">Simulation</option>
                    <option value="Monitoring">Monitoring</option>
                </select>
                <input
                    className="input input-bordered w-24"
                    type="number"
                    value={maxAttempts}
                    onChange={e => setMaxAttempts(Number(e.target.value))}
                    min={1}
                    max={10}
                />
                <button 
                    className="btn btn-primary"
                    onClick={() => mutation.mutate()}
                    disabled={mutation.isPending}
                >
                    {mutation.isPending ? <span className="loading loading-spinner"></span> : 'Create Task'}
                </button>
            </div>
        </div>
    )
}