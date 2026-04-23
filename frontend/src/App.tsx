import { useQuery } from '@tanstack/react-query'
import { getAllTasks } from './api/tasks'
import { TaskForm } from './components/TaskForm'
import { TaskTable } from './components/TaskTable'
import { TaskMetrics } from './components/TaskMetrics'
import { TaskDescription } from './components/TaskDescription'


function App() {
  const { data: tasks, isLoading } = useQuery({
    queryKey: ['tasks'],
    queryFn: getAllTasks,
    refetchInterval: 2000
  })

  return (
  <div style={{ display: 'flex', flexDirection: 'column', gap: '24px', maxWidth: '1152px', margin: '0 auto' }}>
  <h1 className="text-4xl font-bold">TaskOrchestrator</h1>
    <TaskForm />
    <TaskDescription />
    <TaskMetrics tasks={tasks ?? []} />
    {isLoading ? (
        <span className="loading loading-spinner loading-lg"></span>
    ) : (
        <TaskTable tasks={tasks ?? []} />
    )} 
</div>
  )
}

export default App