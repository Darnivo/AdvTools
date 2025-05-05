# Python virtual environenmt requirements:
Python version : `3.8.10` **(Important!)**

Library requirements:
- mlagents==0.29.0     `Unity ML-Agents toolkit `
- torch==1.7.1+cpu     `PyTorch (CPU version)`
- tensorboard          `Training visualization`
- numpy                `Data handling`
- matplotlib           `Visualization`
- protobuf==3.20.3     `Required for compatibility`
> Note that some base packages are downgraded

<br>

Training: <br>
Train using `mlagents-learn PATH_TO_YAML_CONFIG_FILE --env=PATH_TO_YOUR_TRAINING_EXE_FILE --num-envs=NUM_OF_ENVIRONMENTS --run-id=RUN_ID_NAME --no-graphics --force` <br>
Visualize training progress using `tensorboard --logdir results`
