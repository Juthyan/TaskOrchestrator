import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { classifyAndEnqueueTask } from '../api/tasks'
import toast from 'react-hot-toast'

export function TaskDescription()
{
    const queryClient = useQueryClient()
    const [description, setDescription] = useState('');
    const [maxAttempts, setMaxAttempts] = useState(3)

    const mutation = useMutation({
        mutationFn: () => classifyAndEnqueueTask(description, maxAttempts),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['tasks'] })
        },
        onError: (error: Error) => toast.error(error.message)

    })

    return (
        <div className="card bg-base-100 shadow p-6">
            <h2 className="text-xl font-bold mb-4">Create Task by Description</h2>
            <div className="flex flex-col gap-4">
                <textarea
                    className="textarea textarea-bordered h-24"
                    placeholder="Describe your task — Not working currently waiting for Api key"
                    value={description}
                    onChange={e => setDescription(e.target.value)}
                />
                <div className="flex gap-4 items-end">
                    <input
                        className="input input-bordered w-24"
                        type="number"
                        value={maxAttempts}
                        onChange={e => setMaxAttempts(Number(e.target.value))}
                        min={1}
                        max={10}
                    />
                    <button
                        className="btn btn-secondary"
                        onClick={() => mutation.mutate()}
                        disabled={mutation.isPending || !description.trim()}
                    >
                        {mutation.isPending ? <span className="loading loading-spinner"></span> : 'Classify & Enqueue'}
                    </button>
                </div>
            </div>
        </div>
    )
}