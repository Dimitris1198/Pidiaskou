

import sys
import json
import numpy as np
try:
    from sklearn.cluster import BisectingKMeans
except ImportError as e:
    print("An ImportError occurred.")
    print(f"Name: {e.name}")
    sys.exit()


def main():
    try:
        received = input()
        parsed_data= json.loads(received)
        cords=[]
        for cord in parsed_data['positions']:
            cords.append([cord['x'],cord['y'],cord['z']])
        to_np = np.array(cords)
        kmeans = BisectingKMeans(n_clusters=3,max_iter=500, random_state=0, n_init="auto").fit(to_np)
        sys.stdout.write(json.dumps({"peaks":kmeans.cluster_centers_.tolist()}))
    except Exception as err:
        sys.stdout.write(str(err))
    
main()