import { useReactTable, getCoreRowModel, flexRender, createColumnHelper } from '@tanstack/react-table'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import type { Task } from '../types/task'
import { cancelTask, restartTask } from '../api/tasks'
import toast from 'react-hot-toast'


interface TaskTableProps {
    tasks: Task[]
}

const columnHelper = createColumnHelper<Task>()

export function TaskTable({ tasks }: TaskTableProps) {
    const queryClient = useQueryClient()

    const cancelMutation = useMutation({
        mutationFn: (id: string) => cancelTask(id),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tasks'] }),
        onError: (error: Error) => toast.error(error.message)
    })

    const restartMutation = useMutation({
        mutationFn: (id: string) => restartTask(id),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['tasks'] }),
        onError: (error: Error) => toast.error(error.message)
    })

    const columns = [
        columnHelper.accessor('id', { header: 'ID' }),
        columnHelper.accessor('status', { header: 'Status' }),
        columnHelper.accessor('type', { header: 'Type' }),
        columnHelper.accessor('attempts', { header: 'Attempts' }),
        columnHelper.accessor('maxAttempts', { header: 'Max' }),
        columnHelper.display({
            id: 'actions',
            header: 'Actions',
            cell: ({ row }) => (
                <div className="flex gap-2">
                    <button 
                        className="btn btn-sm btn-error"
                        onClick={() => cancelMutation.mutate(row.original.id)}
                    >
                        Cancel
                    </button>
                    <button 
                        className="btn btn-sm btn-warning"
                        onClick={() => restartMutation.mutate(row.original.id)}
                    >
                        Restart
                    </button>
                </div>
            )
        })
    ]

    const table = useReactTable({
        data: tasks,
        columns,
        getCoreRowModel: getCoreRowModel(),
    })

    return (
        <div className="overflow-x-auto mt-8">
            <table className="table">
                <thead>
                    {table.getHeaderGroups().map(headerGroup => (
                        <tr key={headerGroup.id}>
                            {headerGroup.headers.map(header => (
                                <th key={header.id}>
                                    {flexRender(header.column.columnDef.header, header.getContext())}
                                </th>
                            ))}
                        </tr>
                    ))}
                </thead>
                <tbody>
                    {table.getRowModel().rows.map(row => (
                        <tr key={row.id}>
                            {row.getVisibleCells().map(cell => (
                                <td key={cell.id}>
                                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                </td>
                            ))}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    )
}