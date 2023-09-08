from track_generator import TrackGenerator
import random
import math
from datetime import datetime

current_time=datetime.now().strftime("%H%M%S")


default_cfg = {
            'seed': random.random(),
            'min_corner_radius': 3,
            'max_frequency': 7,
            'amplitude': 1 / 3,
            'check_self_intersection': True,
            'starting_amplitude': 0.4,
            'rel_accuracy': 0.005,
            'margin': 0,
            'starting_straight_length': 6,
            'starting_straight_downsample': 2,
            'min_cone_spacing': 3 * math.pi / 16,
            'max_cone_spacing': 3,
            'track_width': 5,
            'cone_spacing_bias': 0.5,
            'starting_cone_spacing': 0.5
        }


random_track = TrackGenerator(default_cfg)
random_track.generate_path_w_params(random, 100, 1, 7, 1/3)

for i in range(1):
    random_track.write_to_csv(f"{current_time}track{i}.csv",*random_track(),overwrite=True)
    print(current_time)

