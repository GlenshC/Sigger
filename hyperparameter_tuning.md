# Hyperparameter Tuning - CLI Commands

Run these 5 configurations in parallel (separate terminals) to perform hyperparameter tuning. Each will save its own model cache and results JSON.

## Commands

```bash
# Config 1: LR=0.001, Hidden=256, Batch=64, Epochs=50 (Slow convergence)
dotnet run -- --train --test --lr 0.001 --hidden 256 --batch 64 --epochs 50

# Config 2: LR=0.01, Hidden=256, Batch=64, Epochs=50 (Best - expected ~97.4% test acc)
dotnet run -- --train --test --lr 0.01 --hidden 256 --batch 64 --epochs 50

# Config 3: LR=0.1, Hidden=256, Batch=64, Epochs=50 (Unstable)
dotnet run -- --train --test --lr 0.1 --hidden 256 --batch 64 --epochs 50

# Config 4: LR=0.01, Hidden=128, Batch=64, Epochs=50 (Underfitting)
dotnet run -- --train --test --lr 0.01 --hidden 128 --batch 64 --epochs 50

# Config 5: LR=0.01, Hidden=512, Batch=64, Epochs=50 (Slower, similar)
dotnet run -- --train --test --lr 0.01 --hidden 512 --batch 64 --epochs 50
```

## Expected Outputs

Each run will create:
- **Model cache**: `model_cache/model_cache_{LR}_{HIDDEN}_{BATCH}_{EPOCHS}.json`
- **Results log**: `results/{LR}_{HIDDEN}_{BATCH}_{EPOCHS}_TRAIN.json`

Example filenames:
- `model_cache/model_cache_0p001_256_64_50.json`
- `model_cache/model_cache_0p01_256_64_50.json`
- `model_cache/model_cache_0p1_256_64_50.json`
- `model_cache/model_cache_0p01_128_64_50.json`
- `model_cache/model_cache_0p01_512_64_50.json`

## Expected Results (from rubric table)

| LR   | Hidden | Batch | Epochs | Val Acc | Test Acc | Notes          |
|------|--------|-------|--------|---------|----------|----------------|
| 0.001 | 256    | 64    | 50     | 96.2%   | 95.8%    | Slow convergence |
| **0.01** | **256** | **64** | **50** | **97.8%** | **97.4%** | **Best**         |
| 0.1   | 256    | 64    | 50     | 94.1%   | 93.5%    | Unstable       |
| 0.01  | 128    | 64    | 50     | 96.8%   | 96.3%    | Underfitting   |
| 0.01  | 512    | 64    | 50     | 97.6%   | 97.2%    | Slower, similar |

## Aggregate Results

After all 5 runs complete, you can compare results:

```bash
# View all results
cat results/*.json | jq '.Metrics | {LR: .Hyperparameters.LearningRate, Hidden: .Hyperparameters.HiddenSize, ValAcc: .ValidationAccuracy, TestAcc: .TestAccuracy}'

# Or use Python to create a summary table
python3 -c "
import json, glob
for f in sorted(glob.glob('results/*_TRAIN.json')):
    with open(f) as fp:
        d = json.load(fp)
    hp = d['Hyperparameters']
    m = d['Metrics']
    print(f\"LR={hp['LearningRate']:6} Hidden={hp['HiddenSize']:4} Batch={hp['BatchSize']:3} Epochs={hp['Epochs']:2} Val={m['ValidationAccuracy']:.1%} Test={m['TestAccuracy']:.1%}\")
"
```

## Running in Parallel (PowerShell)

```powershell
# Run all 5 in parallel (each in its own background job)
$configs = @(
    @{lr=0.001; hidden=256; batch=64; epochs=50},
    @{lr=0.01;  hidden=256; batch=64; epochs=50},
    @{lr=0.1;   hidden=256; batch=64; epochs=50},
    @{lr=0.01;  hidden=128; batch=64; epochs=50},
    @{lr=0.01;  hidden=512; batch=64; epochs=50}
)

$jobs = foreach ($c in $configs) {
    Start-Job -ScriptBlock {
        param($lr, $hidden, $batch, $epochs)
        dotnet run -- --train --test --lr $lr --hidden $hidden --batch $batch --epochs $epochs
    } -ArgumentList $c.lr, $c.hidden, $c.batch, $c.epochs
}

# Wait for all to complete
$jobs | Wait-Job | Receive-Job
```

## Notes

- Each run takes ~5-15 minutes per epoch depending on CPU (50 epochs = ~4-12 minutes per config on modern AVX2 CPU)
- Total time for all 5 in parallel: ~12 minutes (bound by slowest config)
- Sequential would take ~40-60 minutes
- The `model_cache` directory allows concurrent runs without cache conflicts