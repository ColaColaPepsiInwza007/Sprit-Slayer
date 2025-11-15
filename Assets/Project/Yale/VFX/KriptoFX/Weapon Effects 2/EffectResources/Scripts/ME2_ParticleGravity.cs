using MeshEffects2;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;


public class ME2_ParticleGravity : MonoBehaviour
                                 //, ME2_IScriptInstance
{
    public Transform Target;
    public float Force = 1;

    public Vector2 ForceByDistanceRemap = new Vector2(0, 1f);
   
    ParticleSystem             _ps;
    private UpdateParticlesJob _job = new UpdateParticlesJob();

    void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    void OnParticleUpdateJobScheduled()
    {
        _job.CurrentForce         = Time.deltaTime * Force;
        _job.TargetPosition       = Target.position;
        _job.ForceByDistanceRemap = ForceByDistanceRemap;
        _job.Schedule(_ps);
    }
    struct UpdateParticlesJob : IJobParticleSystem
    {
        public float   CurrentForce;
        public Vector3 TargetPosition;
        public Vector2 ForceByDistanceRemap;

        public void Execute(ParticleSystemJobData particles)
        {
            int particleCount       = particles.count;
            var particlesVelocities = particles.velocities;
            var positions           = particles.positions;

            for (int i = 0; i < particleCount; i++)
            {
                var directionToTarget = Vector3.Normalize(TargetPosition - positions[i]);
                var distanceForce     = Mathf.SmoothStep(ForceByDistanceRemap.x, ForceByDistanceRemap.y, Vector3.Distance(TargetPosition, positions[i]));
                var seekForce         = directionToTarget * CurrentForce * distanceForce;

                particlesVelocities[i] += seekForce;
            }
        }
    }
}
